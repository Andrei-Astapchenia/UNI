using System;
using System.Collections.Generic;
using System.Text;

namespace RegexHelper.Engine;
public class ExpressionSyntaxException : Exception
{
    public int ErrorPosition { get; }
    public ExpressionSyntaxException(string message, int errorPosition) : base(message) => ErrorPosition = errorPosition;
}

public class ExpressionParser
{
    private readonly string sourceText;
    private int currentIndex;

    public ExpressionParser(string sourceText) => this.sourceText = sourceText;
    private bool HasMoreChars => currentIndex < sourceText.Length;
    private char CurrentChar => sourceText[currentIndex];

    public TreeNode ParseWholeExpression()
    {
        TreeNode result = ParseAlternation();
        if (HasMoreChars)
            throw new ExpressionSyntaxException( $"Неожиданный символ '{CurrentChar}'", currentIndex);
        return result;
    }

    private TreeNode ParseAlternation()
    {
        var branches = new List<TreeNode> { ParseConcatenation() };

        while (HasMoreChars && CurrentChar == '|')
        {
            currentIndex++;
            branches.Add(ParseConcatenation());
        }

        return branches.Count == 1 ? branches[0] : new AlternationNode { Branches = branches };
    }

    private TreeNode ParseConcatenation()
    {
        var sequence = new SequenceNode();

        while (HasMoreChars && CurrentChar != '|' && CurrentChar != ')')
        {
            sequence.Items.Add(ParseRepeat());
        }
        return sequence.Items.Count == 1 ? sequence.Items[0] : sequence;
    }
    private TreeNode ParseRepeat()
    {
        TreeNode atom = ParseAtom();
        if (!HasMoreChars) return atom;
        char symbol = CurrentChar;
        if (symbol == '*') { currentIndex++; return new RepeatNode(atom, 0, -1); }
        if (symbol == '+') { currentIndex++; return new RepeatNode(atom, 1, -1); }
        if (symbol == '?') { currentIndex++; return new RepeatNode(atom, 0, 1); }
        if (symbol == '{') return ParseCurlyQuantifier(atom);

        return atom;
    }

    private TreeNode ParseCurlyQuantifier(TreeNode atom)
    {
        int startPosition = currentIndex;
        currentIndex++;
        int minCount = ReadNumber();
        int maxCount;
        if (!HasMoreChars)
            throw new ExpressionSyntaxException("Ожидалась '}'", currentIndex);
        if (CurrentChar == '}')
        {
            maxCount = minCount;
            currentIndex++;
        }
        else if (CurrentChar == ',')
        {
            currentIndex++;
            if (HasMoreChars && CurrentChar == '}')
            {
                maxCount = -1;
                currentIndex++;
            }
            else
            {
                maxCount = ReadNumber();
                if (!HasMoreChars || CurrentChar != '}')
                    throw new ExpressionSyntaxException("Ожидалась '}'", currentIndex);
                currentIndex++;
            }
        }
        else
        {
            throw new ExpressionSyntaxException("Ожидалась '}' или ','", currentIndex);
        }
        if (maxCount != -1 && minCount > maxCount)
            throw new ExpressionSyntaxException($"В квантификаторе {minCount},{maxCount} минимум больше максимума", startPosition);
        return new RepeatNode(atom, minCount, maxCount);
    }

    private int ReadNumber()
    {
        int numberStart = currentIndex;
        while (HasMoreChars && char.IsDigit(CurrentChar)) currentIndex++;
        if (currentIndex == numberStart)
            throw new ExpressionSyntaxException("Ожидалось число", currentIndex);
        return int.Parse(sourceText.AsSpan(numberStart, currentIndex - numberStart));
    }
    private TreeNode ParseAtom()
    {
        if (!HasMoreChars)
            throw new ExpressionSyntaxException("Неожиданный конец выражения", currentIndex);
        char symbol = CurrentChar;
        if (symbol == '(')
        {
            currentIndex++;
            TreeNode inner = ParseAlternation();
            if (!HasMoreChars || CurrentChar != ')')
                throw new ExpressionSyntaxException("Ожидалась ')'", currentIndex);
            currentIndex++;
            return inner;
        }
        if (symbol == '[') return ParseCharClass();
        if (symbol == '\\') return ParseEscape();
        if (symbol == '.')
        {
            currentIndex++;
            return new AnyCharNode();
        }

        if (symbol == '*' || symbol == '+' || symbol == '?' ||
            symbol == ')' || symbol == '|' || symbol == '{')
        {
            throw new ExpressionSyntaxException($"Неожиданный оператор '{symbol}'", currentIndex);
        }
        currentIndex++;
        return new LiteralNode(symbol);
    }

    private TreeNode ParseEscape()
    {
        int escapeStart = currentIndex;
        currentIndex++; 
        if (!HasMoreChars)
            throw new ExpressionSyntaxException( "Незавершённая escape-последовательность", escapeStart);
        char escapeSymbol = CurrentChar;
        currentIndex++;
        const string knownEscapes = "dDwWsSnrt\\.";
        bool isKnownEscape = knownEscapes.IndexOf(escapeSymbol) >= 0;
        bool isEscapedPunctuation = "()[]{}.*+?|\\^-$".IndexOf(escapeSymbol) >= 0;

        if (isKnownEscape) return new EscapeNode(escapeSymbol);
        if (isEscapedPunctuation) return new LiteralNode(escapeSymbol);

        throw new ExpressionSyntaxException( $"Неизвестная escape-последовательность \\{escapeSymbol}", escapeStart);
    }
    private TreeNode ParseCharClass()
    {
        int classStart = currentIndex;
        currentIndex++;
        bool isNegated = false;
        if (HasMoreChars && CurrentChar == '^') { isNegated = true; currentIndex++; }
        var charClass = new CharClassNode(isNegated);
        while (true)
        {
            if (!HasMoreChars)
                throw new ExpressionSyntaxException("Незавершённый класс символов", classStart);
            if (CurrentChar == ']')
            {
                currentIndex++;
                return charClass;
            }
            char firstChar = ReadCharInClass(classStart, charClass);
            bool isRangeStart = HasMoreChars && CurrentChar == '-' &&  currentIndex + 1 < sourceText.Length && sourceText[currentIndex + 1] != ']';
            if (isRangeStart)
            {
                currentIndex++; 
                char lastChar = CurrentChar;
                currentIndex++;
                if (lastChar < firstChar)
                    throw new ExpressionSyntaxException( $"Некорректный диапазон {firstChar}-{lastChar}",currentIndex - 3);

                charClass.AllowedRanges.Add(new SymbolRange(firstChar, lastChar));
            }
            else
            {
                charClass.AllowedRanges.Add(new SymbolRange(firstChar, firstChar));
            }
        }
    }
    private char ReadCharInClass(int classStart, CharClassNode charClass)
    {
        if (CurrentChar == '\\')
        {
            currentIndex++;
            if (!HasMoreChars)
                throw new ExpressionSyntaxException( "Незавершённая escape-последовательность", classStart);
            char escapeSymbol = CurrentChar;
            currentIndex++;
            switch (escapeSymbol)
            {
                case 'd':
                    charClass.AllowedRanges.Add(new SymbolRange('0', '9'));
                    return '\0';
                case 'w':
                    charClass.AllowedRanges.Add(new SymbolRange('a', 'z'));
                    charClass.AllowedRanges.Add(new SymbolRange('A', 'Z'));
                    charClass.AllowedRanges.Add(new SymbolRange('0', '9'));
                    charClass.AllowedRanges.Add(new SymbolRange('_', '_'));
                    return '\0';
                case 's':
                    charClass.AllowedRanges.Add(new SymbolRange(' ', ' '));
                    charClass.AllowedRanges.Add(new SymbolRange('\t', '\t'));
                    charClass.AllowedRanges.Add(new SymbolRange('\n', '\n'));
                    charClass.AllowedRanges.Add(new SymbolRange('\r', '\r'));
                    return '\0';
                default:
                    return escapeSymbol;
            }
        }
        char literal = CurrentChar;
        currentIndex++;
        return literal;
    }
}