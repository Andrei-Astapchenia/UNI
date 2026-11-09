using System;
using System.Collections.Generic;
using System.Text;

namespace RegexHelper.Engine;

public class ExpressionCheckResult
{
    public bool IsValid { get; init; }
    public TreeNode? RootNode { get; init; }
    public string? ErrorMessage { get; init; }
    public int ErrorPosition { get; init; } = -1;
}

public static class Regex
{
    public static ExpressionCheckResult CheckExpression(string expression)
    {
        if (string.IsNullOrEmpty(expression))
        {
            return new ExpressionCheckResult
            {
                IsValid = false,
                ErrorMessage = "Пустое выражение не является регулярным.",
                ErrorPosition = 0
            };
        }

        try
        {
            TreeNode root = new ExpressionParser(expression).ParseWholeExpression();
            return new ExpressionCheckResult { IsValid = true, RootNode = root };
        }
        catch (ExpressionSyntaxException ex)
        {
            return new ExpressionCheckResult
            {
                IsValid = false,
                ErrorMessage = ex.Message,
                ErrorPosition = ex.ErrorPosition
            };
        }
    }

    public static List<FoundMatch> Search(TreeNode rootNode, string text) => new SearchEngine(rootNode).FindAll(text);
}
