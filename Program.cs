using System;
using System.IO;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

class Program
{
    static Dictionary<string, Color> ColorMap = new Dictionary<string, Color>(StringComparer.OrdinalIgnoreCase)
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

    static void Main()
    {
        string filePath = @"D:\sharps\1 lab\ColorInTheText\aeroport.txt";

        string text = File.ReadAllText(filePath);
        Console.WriteLine("Файл загружен");
        List<string> words = GetWords(text);
        Console.WriteLine($"Распознано {words.Count} слов");

        List<Color> foundColors = FindColorsInText(words);
        Console.WriteLine($"Найдено {foundColors.Count} цветов:");
    }

    static List<string> GetWords(string text)
    {
        char[] separators = { ' ', ',', '.', '!', '?', ';', ':', '\t', '\n', '\r', '(', ')', '[', ']', '{', '}', '"', '\'' };

        return text.Split(separators, StringSplitOptions.RemoveEmptyEntries)
                   .Select(word => word.ToLower())
                   .ToList();
    }

    static List<Color> FindColorsInText(List<string> words)
    {
        List<Color> foundColors = new List<Color>();

        foreach (string word in words)
        {
            foreach (var colorMapping in ColorMap)
            {
                if (word.Contains(colorMapping.Key, StringComparison.OrdinalIgnoreCase))
                {
                    foundColors.Add(colorMapping.Value);
                    break;
                }
            }
        }

        return foundColors;
    }
}