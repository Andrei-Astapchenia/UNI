using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using RegexHelper.Engine;

namespace RegexHelper;

public partial class MainWindow : Window
{
    public MainWindow() => InitializeComponent();

    private void CheckButton_Click(object sender, RoutedEventArgs e)
    {
        StatusLabel.Text = "";
        string expression = ExpressionInput.Text ?? "";
        string textToSearch = ReadTextFromRichTextBox();

        if (string.IsNullOrEmpty(textToSearch) || textToSearch.Trim('\r', '\n').Length == 0)
        {
            MessageBox.Show( "Строка для поиска пуста. Введите текст.", "Предупреждение",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return;
        }
        ExpressionCheckResult checkResult = Regex.CheckExpression(expression);
        if (!checkResult.IsValid)
        {
            MessageBox.Show($"Выражение не является корректным регулярным выражением:\n" + $"{checkResult.ErrorMessage}\nПозиция: {checkResult.ErrorPosition}","Ошибка",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return;
        }

        List<FoundMatch> matches;
        try
        {
            matches = Regex.Search(checkResult.RootNode!, textToSearch);
        }
        catch (Exception ex)
        {
            MessageBox.Show("Ошибка при поиске: " + ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        ClearAllHighlights();
        if (matches.Count == 0)
        {
            MessageBox.Show("Совпадений не найдено.", "Результат", MessageBoxButton.OK, MessageBoxImage.Information);
            StatusLabel.Text = "Совпадений нет.";
            return;
        }
        HighlightAllMatches(matches);
        StatusLabel.Text = $"Найдено совпадений: {matches.Count}";
    }
    private string ReadTextFromRichTextBox()
    {
        var fullRange = new TextRange(
            TextInputArea.Document.ContentStart,
            TextInputArea.Document.ContentEnd);
        return fullRange.Text;
    }
    private void ClearAllHighlights()
    {
        var fullRange = new TextRange(
            TextInputArea.Document.ContentStart,
            TextInputArea.Document.ContentEnd);
        fullRange.ApplyPropertyValue(TextElement.BackgroundProperty, Brushes.Transparent);
        fullRange.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Black);
    }
    private void HighlightAllMatches(List<FoundMatch> matches)
    {
        TextPointer documentStart = TextInputArea.Document.ContentStart;
        foreach (FoundMatch match in matches)
        {
            TextPointer? rangeStart = FindPointerAtOffset(documentStart, match.StartIndex);
            TextPointer? rangeEnd = FindPointerAtOffset(documentStart, match.EndIndex);
            if (rangeStart == null || rangeEnd == null) continue;
            var range = new TextRange(rangeStart, rangeEnd);
            range.ApplyPropertyValue(TextElement.BackgroundProperty, Brushes.Yellow);
            range.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Black);
        }
    }
    private static TextPointer? FindPointerAtOffset(TextPointer documentStart, int targetOffset)
    {
        int countedChars = 0;
        TextPointer? current = documentStart;
        while (current != null)
        {
            if (current.GetPointerContext(LogicalDirection.Forward) == TextPointerContext.Text)
            {
                string textRun = current.GetTextInRun(LogicalDirection.Forward);
                if (countedChars + textRun.Length >= targetOffset)
                {
                    int offsetInsideRun = targetOffset - countedChars;
                    return current.GetPositionAtOffset(offsetInsideRun);
                }
                countedChars += textRun.Length;
            }
            current = current.GetNextContextPosition(LogicalDirection.Forward);
        }
        return null;
    }
}