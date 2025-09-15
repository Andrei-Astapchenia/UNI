using System;
using System.IO;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;

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
        string filePath = @"D:\sharps\1lab\ColorInTheText\aeroport.txt";

        string text = File.ReadAllText(filePath);
        Console.WriteLine("Файл загружен");
        text = text.ToLower();
        string[] splittedText = Regex.Split(text, @"[\p{P}\s\r\n]+");
        Console.WriteLine($"Распознано {splittedText.Length} слов");

        List<Color> foundColors = FindColorsInText(splittedText);
        Console.WriteLine($"Найдено {foundColors.Count} цветов:");
    }


    static List<Color> FindColorsInText(string[] words)
    {
        List<Color> foundColors = new List<Color>();
        foreach (string word in words)
        {
            foreach (var colorMapping in ColorMap)
            {
                if (word.Contains(colorMapping.Key))
                {
                    foundColors.Add(colorMapping.Value);
                    break; 
                }
            }
        }
        return foundColors;
    }
}