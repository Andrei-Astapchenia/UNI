using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

struct GeneticData
{
    public string protein;
    public string organism;
    public string amino_acids;
}

class Program
{
    static void Main()
    {
        List<GeneticData> proteins = new List<GeneticData>();
        string[] lines = File.ReadAllLines("sequences.txt");
        string[] commands = File.ReadAllLines("commands.txt");
        foreach (string line in lines)
        {
            if (string.IsNullOrWhiteSpace(line)) continue;
            string[] parts = line.Split('\t');
            if (parts.Length == 3) {
                GeneticData data = new GeneticData();
                data.protein = parts[0];
                data.organism = parts[1];
                data.amino_acids = parts[2];
                proteins.Add(data);
            }
        }
        using (StreamWriter output = new StreamWriter("genedata.txt"))
        {
            output.WriteLine("Генетический поиск");
            for (int i = 0; i < commands.Length; i++)
            {
                string command = commands[i];
                output.WriteLine(new string('-', 100));
                string[] commandParts = command.Split('\t');
                output.Write($"{(i + 1).ToString("D3")}");
                for (int j = 0; j < commandParts.Length; j++) {
                    output.Write($"\t{commandParts[j]}");
                }
                output.WriteLine();
                if (commandParts.Length > 0)
                {
                    string operation = commandParts[0];
                    switch (operation)
                    {
                        case "search":
                            if (commandParts.Length > 1)
                            {
                                string searchSequence = commandParts[1];
                                string decoded = searchSequence;
                                if (HasDigits(searchSequence))
                                {
                                    decoded = RLD(searchSequence);
                                }
                                SearchOperation(proteins, decoded, output);
                            }
                            break;
                        case "diff":
                            if (commandParts.Length > 2)
                            {
                                DiffOperation(proteins, commandParts[1], commandParts[2], output);
                            }
                            break;
                        case "mode":
                            if (commandParts.Length > 1)
                            {
                                ModeOperation(proteins, commandParts[1], output);
                            }
                            break;
                    }
                }
            }
            output.WriteLine(new string('-', 100));
        }
        Console.WriteLine("Результат записан в genedata.txt");
    }

    static string RLD(string Sequence)
    {
        string result = ""; //string.Empty
        int numberOFsymbol = 0;
        while (numberOFsymbol < Sequence.Length)
        {
            char symbol = Sequence[numberOFsymbol];
            if (symbol >= '1' && symbol <= '9') 
            {
             int countOFacids = int.Parse(symbol.ToString());
                numberOFsymbol++;
                char acid = Sequence[numberOFsymbol];
                for (int i = 0; i < countOFacids; i++)
                {
                    result += acid;
                }
            }
            else
            {
                result += symbol;
            }
            numberOFsymbol++;
        }
        return result;
    }
    static string RLE(string Sequence)
        {
            if (Sequence == "") return "";
            string result = "";
            int count = 1;
            char currentAcid = Sequence[0];
            for (int i = 1; i < Sequence.Length; i++)
            {
                if (Sequence[i] == currentAcid)
                {
                    count++;
                }
                else
                {
                    if (count > 2)
                    {
                        result += count.ToString() + currentAcid;
                    }
                    else
                    {
                        for (int j = 0; j < count; j++)
                        {
                            result += currentAcid;
                        }
                    }
                    count = 1;
                    currentAcid = Sequence[i];
                }
            }
            if (count > 2)
            {
                result += count.ToString() + currentAcid;
            }
            else
            {
                for (int j = 0; j < count; j++)
                {
                    result += currentAcid;
                }
            }
        return result;
        }
    static bool HasDigits(string Sequence)
        {
            foreach (char c in Sequence)
            {
                if (c >= '0' && c <= '9') return true;
            }
            return false;
        }
    static void SearchOperation(List<GeneticData> proteins, string searchSequence, StreamWriter output)
    {
        bool found = false;
        foreach (GeneticData data in proteins)
        {
            if (data.amino_acids.Contains(searchSequence))
            {
                output.WriteLine($"organism\t protein");
                output.WriteLine($"{data.organism}\t{data.protein}");
                found = true;
            }
        }
        if (!found)
        {
            output.WriteLine("Не найдено");
        }
    }
    static void DiffOperation(List<GeneticData> proteins, string protein1, string protein2, StreamWriter output)
    {
        GeneticData found1 = new GeneticData();
        GeneticData found2 = new GeneticData();
        bool foundProtein1 = false;
        bool foundProtein2 = false;
        foreach (GeneticData data in proteins)
        {
            if (data.protein == protein1)
            {
                found1 = data;
                foundProtein1 = true;
            }
            if (data.protein == protein2)
            {
                found2 = data;
                foundProtein2 = true;
            }
        }
        if (!foundProtein1 || !foundProtein2)
        {
            output.Write("MISSING: ");
            if (!foundProtein1) output.Write(protein1 + " ");
            if (!foundProtein2) output.Write(protein2);
            output.WriteLine();
            return;
        }
        string sequence1 = found1.amino_acids;
        string sequence2 = found2.amino_acids;
        int minLength = Math.Min(sequence1.Length, sequence2.Length);
        int difference = 0;

        for (int i = 0; i < minLength; i++)
        {
            if (sequence1[i] != sequence2[i]) difference++;
        }

        difference += Math.Abs(sequence1.Length - sequence2.Length);
        output.WriteLine($"amino-acids difference:\n {difference}");
    }
    static void ModeOperation(List<GeneticData> proteins, string proteinName, StreamWriter output)
    {
        GeneticData foundProtein = new GeneticData();
        bool found = false;

        foreach (GeneticData data in proteins)
        {
            if (data.protein == proteinName)
            {
                foundProtein = data;
                found = true;
                break;
            }
        }

        if (!found)
        {
            output.WriteLine($"MISSING: {proteinName}");
            return;
        }
        string sequence = foundProtein.amino_acids;
        int[] counts = new int[26]; // A - Z
        string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        foreach (char acid in sequence)
        {
            int index = alphabet.IndexOf(acid);
            if (index >= 0)
            {
                counts[index]++;
            }
        }
        int maxCount = 0;
        for (int i = 0; i < counts.Length; i++)
        {
            if (counts[i] > maxCount)
            {
                maxCount = counts[i];
            }
        }
        for (int i = 0; i < counts.Length; i++)
        {
            if (counts[i] == maxCount)
            {
                char mostCommonAcid = alphabet[i];
                output.WriteLine($"amino-acid occurs:\n{mostCommonAcid} = {maxCount}");
                return;
            }
        }


    }
}