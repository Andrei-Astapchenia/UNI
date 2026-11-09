using System;
using System.Collections.Generic;
using System.Text;

namespace RegexHelper.Engine;
public abstract class TreeNode { }
public class LiteralNode : TreeNode
{
    public char Symbol { get; }
    public LiteralNode(char symbol) => Symbol = symbol;
}
public class AnyCharNode : TreeNode { }

//диапазон символов
public readonly record struct SymbolRange(char FirstChar, char LastChar)
{
    public bool Contains(char c) => c >= FirstChar && c <= LastChar;
}

public class CharClassNode : TreeNode
{
    public bool IsNegated { get; }  
    public List<SymbolRange> AllowedRanges { get; } = new();

    public CharClassNode(bool isNegated) => IsNegated = isNegated;

    public bool Matches(char c)
    {
        bool foundInRanges = false;
        foreach (var range in AllowedRanges)
        {
            if (range.Contains(c)) { foundInRanges = true; break; }
        }
        return IsNegated ? !foundInRanges : foundInRanges;
    }
}

public class EscapeNode : TreeNode
{
    public char EscapeKind { get; }   // 'd', 'w', 's', 'D', 'W', 'S', 'n', 't', 'r', '\\', '.'
    public EscapeNode(char escapeKind) => EscapeKind = escapeKind;

    public bool Matches(char c) => EscapeKind switch
    {
        'd' => char.IsDigit(c),
        'D' => !char.IsDigit(c),
        'w' => char.IsLetterOrDigit(c) || c == '_',
        'W' => !(char.IsLetterOrDigit(c) || c == '_'),
        's' => char.IsWhiteSpace(c),
        'S' => !char.IsWhiteSpace(c),
        'n' => c == '\n',
        't' => c == '\t',
        'r' => c == '\r',
        '\\' => c == '\\',
        '.' => c == '.',
        _ => false
    };
}
public class SequenceNode : TreeNode
{
    public List<TreeNode> Items { get; set; } = new();
}

public class AlternationNode : TreeNode
{
    public List<TreeNode> Branches { get; set; } = new();
}

public class RepeatNode : TreeNode
{
    public TreeNode InnerNode { get; }
    public int MinCount { get; }
    public int MaxCount { get; }   

    public RepeatNode(TreeNode innerNode, int minCount, int maxCount)
    {
        InnerNode = innerNode;
        MinCount = minCount;
        MaxCount = maxCount;
    }
}