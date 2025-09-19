using System;
using System.IO;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using System.Drawing.Imaging;

class Program
{
    static Dictionary<string, Color> Colors = new Dictionary<string, Color>(StringComparer.OrdinalIgnoreCase)
    {
        { "красн", Color.Red },
        { "ал", Color.Crimson },
        { "багр", Color.DarkRed },
        { "зелен", Color.Green },
        { "изумруд", Color.MediumSeaGreen },
        { "малахит", Color.MediumSeaGreen },
        { "син", Color.Blue },
        { "голуб", Color.LightBlue },
        { "лазур", Color.LightSkyBlue },
        { "ультрамарин", Color.Blue },
        { "желт", Color.Yellow },
        { "золот", Color.Gold },
        { "лимон", Color.LemonChiffon },
        { "бел", Color.White },
        { "черн", Color.Black },
        { "сер", Color.Gray },
        { "фиолетов", Color.Purple },
        { "лилов", Color.Purple },
        { "оранжев", Color.Orange },
        { "коричнев", Color.Brown },
        { "розов", Color.Pink },
        { "бирюз", Color.Turquoise },
    };
    static Dictionary<string, string[]> ColorEndings = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
    {
        { "красн", new[] { "ый", "ая", "ое", "ые", "енький", "оватый", "" } },
        { "ал", new[] { "ый", "ая", "ое", "ые", "" } },
        { "багр", new[] { "овый", "ая", "ое", "ые", "" } },
        { "зелен", new[] { "ый", "ая", "ое", "ые", "енький", "оватый", "" } },
        { "изумруд", new[] { "ный", "ная", "ное", "ные", "" } },
        { "малахит", new[] { "овый", "ая", "ое", "ые", "" } },
        { "син", new[] { "ий", "яя", "ее", "ие", "енький", "" } },
        { "голуб", new[] { "ой", "ая", "ое", "ые", "енький", "" } },
        { "лазур", new[] { "ный", "ная", "ное", "ные", "" } },
        { "ультрамарин", new[] { "овый", "ая", "ое", "ые", "" } },
        { "желт", new[] { "ый", "ая", "ое", "ые", "енький", "оватый", "" } },
        { "золот", new[] { "ой", "ая", "ое", "ые", "истый", "" } },
        { "лимон", new[] { "ный", "ная", "ное", "ные", "" } },
        { "бел", new[] { "ый", "ая", "ое", "ые", "енький", "" } },
        { "черн", new[] { "ый", "ая", "ое", "ые", "енький", "" } },
        { "сер", new[] { "ый", "ая", "ое", "ые", "енький", "" } },
        { "фиолетов", new[] { "ый", "ая", "ое", "ые", "" } },
        { "лилов", new[] { "ый", "ая", "ое", "ые", "" } },
        { "оранжев", new[] { "ый", "ая", "ое", "ые", "" } },
        { "коричнев", new[] { "ый", "ая", "ое", "ые", "" } },
        { "розов", new[] { "ый", "ая", "ое", "ые", "енький", "" } },
        { "бирюз", new[] { "овый", "ая", "ое", "ые", "" } },
    };

    static void Main()
    {
        string filePath = @"D:\csharp\1lab\ColorInTheText\";
        string textName = "aeroport.txt";

        string text = File.ReadAllText(filePath+textName);
        Console.WriteLine("Файл загружен");
        text = text.ToLower();
        string[] splittedText = Regex.Split(text, @"[\p{P}\s\r\n]+");
        Console.WriteLine($"Распознано {splittedText.Length} слов");

        List<Color> foundColors = FindColorsInText(splittedText);
        Console.WriteLine($"Найдено {foundColors.Count} цветов:");
        var colorGroups = foundColors.GroupBy(c => c.Name)
                                    .OrderByDescending(g => g.Count());

        foreach (var group in colorGroups)
        {
            Console.WriteLine($"{group.Key}: {group.Count()} раз");
        }
    }
    static Bitmap bitmap = new Bitmap(100, 100);
    static Graphics graphics = Graphics.FromImage(bitmap);
    static SolidBrush brush = new SolidBrush(Color.White);


    static List<Color> FindColorsInText(string[] words)
    {
        List<Color> foundColors = new List<Color>();
        foreach (string word in words)
        {
            if (string.IsNullOrWhiteSpace(word) || word.Length < 3)
                continue;

            foreach (var colorBase in ColorEndings)
            {
                string baseWord = colorBase.Key;
                string[] endings = colorBase.Value; 

                if (word.StartsWith(baseWord))
                {
                    string restOfWord = word.Substring(baseWord.Length);
                    foreach (string ending in endings)
                    {
                        if (restOfWord == ending)
                        {
                            if (Colors.TryGetValue(baseWord, out Color color))
                            {
                                foundColors.Add(color);
                            }
                            break;
                        }
                    }
                    break; 
                }
            }
        }
        return foundColors;
    }

}