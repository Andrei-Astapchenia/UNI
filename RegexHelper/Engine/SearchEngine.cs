using System;
using System.Collections.Generic;
using System.Text;

namespace RegexHelper.Engine;

public class FoundMatch
{
    public int StartIndex { get; }
    public int EndIndex { get; }
    public FoundMatch(int startIndex, int endIndex)
    {
        StartIndex = startIndex;
        EndIndex = endIndex;
    }
    public int Length => EndIndex - StartIndex;
}

public class SearchEngine
{
    private readonly TreeNode rootNode;
    private int stepsUsed;
    private const int MaxSteps = 1_000_000;

    public SearchEngine(TreeNode rootNode) => this.rootNode = rootNode;
    public List<FoundMatch> FindAll(string text)
    {
        var results = new List<FoundMatch>();
        int startIndex = 0;
        while (startIndex <= text.Length)
        {
            FoundMatch? match = FindFirstFrom(text, startIndex);
            if (match == null) break;
            if (match.Length > 0)
            {
                results.Add(match);
                startIndex = match.EndIndex;
            }
            else
            {
                startIndex = match.StartIndex + 1;
            }
        }
        return results;
    }

    public FoundMatch? FindFirstFrom(string text, int startIndex)
    {
        for (int candidate = startIndex; candidate <= text.Length; candidate++)
        {
            stepsUsed = 0;
            HashSet<int> possibleEnds = MatchNode(rootNode, text, candidate);
            if (possibleEnds.Count > 0)
            {
                int maxEnd = 0;
                foreach (var end in possibleEnds) if (end > maxEnd) maxEnd = end;
                return new FoundMatch(candidate, maxEnd);
            }
        }
        return null;
    }
    private HashSet<int> MatchNode(TreeNode node, string text, int position)
    {
        if (++stepsUsed > MaxSteps) return new HashSet<int>();

        return node switch
        {
            LiteralNode literal => MatchLiteral(literal, text, position),
            AnyCharNode => MatchAnyChar(text, position),
            CharClassNode charClass => MatchCharClass(charClass, text, position),
            EscapeNode escape => MatchEscape(escape, text, position),
            SequenceNode sequence => MatchSequence(sequence, text, position),
            AlternationNode alternation => MatchAlternation(alternation, text, position),
            RepeatNode repeat => MatchRepeat(repeat, text, position),
            _ => new HashSet<int>()
        };
    }

    private static HashSet<int> MatchLiteral(LiteralNode node, string text, int position)
    {
        var result = new HashSet<int>();
        if (position < text.Length && text[position] == node.Symbol)
            result.Add(position + 1);
        return result;
    }

    private static HashSet<int> MatchAnyChar(string text, int position)
    {
        var result = new HashSet<int>();
        if (position < text.Length && text[position] != '\n')
            result.Add(position + 1);
        return result;
    }

    private static HashSet<int> MatchCharClass(CharClassNode node, string text, int position)
    {
        var result = new HashSet<int>();
        if (position < text.Length && node.Matches(text[position]))
            result.Add(position + 1);
        return result;
    }

    private static HashSet<int> MatchEscape(EscapeNode node, string text, int position)
    {
        var result = new HashSet<int>();
        if (position < text.Length && node.Matches(text[position]))
            result.Add(position + 1);
        return result;
    }

    private HashSet<int> MatchSequence(SequenceNode sequence, string text, int position)
    {
        var currentPositions = new HashSet<int> { position };

        foreach (TreeNode item in sequence.Items)
        {
            var nextPositions = new HashSet<int>();
            foreach (int pos in currentPositions)
                foreach (int endPos in MatchNode(item, text, pos))
                    nextPositions.Add(endPos);
            currentPositions = nextPositions;
            if (currentPositions.Count == 0) break;
        }
        return currentPositions;
    }

    private HashSet<int> MatchAlternation(AlternationNode node, string text, int position)
    {
        var result = new HashSet<int>();
        foreach (TreeNode branch in node.Branches)
            foreach (int endPos in MatchNode(branch, text, position))
                result.Add(endPos);
        return result;
    }
    private HashSet<int> MatchRepeat(RepeatNode node, string text, int position)
    {
        var result = new HashSet<int>();
        if (node.MinCount == 0) result.Add(position);

        var frontier = new HashSet<int> { position };
        int repetitions = 0;

        int repetitionLimit = node.MaxCount == -1 ? text.Length + 1 : Math.Min(node.MaxCount, text.Length + 1);

        while (frontier.Count > 0 && repetitions < repetitionLimit)
        {
            var nextFrontier = new HashSet<int>();
            foreach (int pos in frontier)
            {
                foreach (int endPos in MatchNode(node.InnerNode, text, pos))
                {
                    if (endPos == pos) continue; 
                    nextFrontier.Add(endPos);
                }
            }
            repetitions++;
            if (repetitions >= node.MinCount)
                foreach (int endPos in nextFrontier)
                    result.Add(endPos);
            if (nextFrontier.Count == 0) break;
            if (node.MaxCount != -1 && repetitions >= node.MaxCount) break;
            frontier = nextFrontier;
        }
        return result;
    }
}