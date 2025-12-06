using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Xml.Serialization;

namespace textParser
{
    public class TextFunction
    {
        private Text text;
        public TextFunction(Text text) {  this.text = text; }
        //1
        public List <Sentence> SortByCountWord()
        {
            return text.Sentences.OrderBy(s=>s.CountOfWordsInSentense).ToList();
        }
        //2
        public List <Sentence> SortByLenghtSentence()
        {
            return text.Sentences.OrderBy(s => s.GetSentence().Length).ToList();
        }
        //3
        public List<string> FindLenghtWordsInQuestions(int wordLength)
        {
            HashSet<string> uniqueWords = new HashSet<string>();

            foreach (var sentence in text.Sentences)
            {
                if (IsQuestionSentence(sentence))
                {
                    foreach (var token in sentence.Tokens)
                    {
                        if (token is Word word && word.Value.Length == wordLength)
                        {
                            uniqueWords.Add(word.Value);
                        }
                    }
                }
            }
            return uniqueWords.ToList();
        }
        private bool IsQuestionSentence(Sentence sentence)
        {
            if (sentence.Tokens.Count > 0)
            {
                var lastToken = sentence.Tokens[sentence.Tokens.Count - 1];
                if (lastToken is Punctuation punctuation && punctuation.Value == "?")
                {
                    return true;
                }
            }
            return false;
        }
        // 4
        public void RemoveWordsWithSoglasnye(int length)
        {
            string soglasnye = "бвгджзклмнпрстфхцчшщbcdfghjklmnpqrstvwxz";

            foreach (var sentence in text.Sentences)
            {
                List<Token> newTokens = new List<Token>();

                foreach (var token in sentence.Tokens)
                {
                    if (token is Word word)
                    {
                        string wordText = word.Value.Trim();
                        if (wordText.Length == length && !string.IsNullOrEmpty(wordText) && soglasnye.Contains(char.ToLower(wordText[0])))
                        {
                            continue;
                        }
                    }
                    newTokens.Add(token);
                }

                sentence.Tokens = newTokens;
            }
        }
        // 5
        public void ReplaceWordsInSentence(int sentenceIndex, int wordLength, string replace)
        {
            if (sentenceIndex < 0 || sentenceIndex >= text.Sentences.Count)
                return;
            Sentence sentence = text.Sentences[sentenceIndex];
            for (int i = 0; i < sentence.Tokens.Count; i++)
            {
                if (sentence.Tokens[i] is Word word && word.Value.Trim().Length == wordLength)
                {
                    sentence.Tokens[i] = new Word(replace);
                }
            }
        }
        //6
        public void RemoveStopWords(string stopWordsFile)
        {
            HashSet<string> stopWords = new HashSet<string>();
            foreach (string line in File.ReadAllLines(stopWordsFile))
            {
                if (!string.IsNullOrWhiteSpace(line))
                {
                    stopWords.Add(line.Trim().ToLower());
                }
            }
            foreach (var sentence in text.Sentences)
            {
                List<Token> newTokens = new List<Token>();
                foreach (var token in sentence.Tokens)
                {
                    if (token is Word word)
                    {
                        string cleanWord = word.Value.Trim().ToLower();
                        if (stopWords.Contains(cleanWord))
                        {
                            continue;
                        }
                    }
                    newTokens.Add(token);
                }
                sentence.Tokens = newTokens;
            }
        }
        //7
        public void ToXml(string xmlFile)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(Text));
            using (FileStream stream = new FileStream(xmlFile, FileMode.Create))
            {
                serializer.Serialize(stream, text);
            }
            Console.WriteLine(" Текст экспортирован в "+xmlFile);
        }
        //8
        public Dictionary<string, Concordance> BuildConcordance()
        {
            Dictionary<string, Concordance> concordance = new Dictionary<string, Concordance>(StringComparer.OrdinalIgnoreCase);

            for (int i = 0; i < text.Sentences.Count; i++)
            {
                Sentence sentence = text.Sentences[i];
                int sentenceNumber = i + 1;
                foreach (var token in sentence.Tokens)
                {
                    if (token is Word word)
                    {
                        string cleanWord = word.Value.Trim().ToLower();
                        if (string.IsNullOrEmpty(cleanWord))
                            continue;
                        if (concordance.ContainsKey(cleanWord))
                        {
                            concordance[cleanWord].Count++;
                            concordance[cleanWord].SentenceNumbers.Add(sentenceNumber);
                        }
                        else 
                        {
                            Concordance newWord = new Concordance(cleanWord)
                            {
                                Count = 1
                            };
                            newWord.SentenceNumbers.Add(sentenceNumber);
                            concordance.Add(cleanWord, newWord);
                        }
                    }
                }
            }

            return concordance;
        }
        public void PrintConcordance(int pageSize=20)
        {
            var concordance = BuildConcordance();
            var sortedWords = concordance.Keys.OrderBy(word => word).ToList();

            Console.WriteLine("Коркорданс");
            Console.WriteLine();

            foreach (var word in sortedWords)
            {
                var ADD = concordance[word];
                string wordLine = $"{word.PadRight(30, '.')}{ADD.Count,5}: ";

                foreach (int num in ADD.SentenceNumbers)
                {
                    wordLine += (num+" ");
                }

                Console.WriteLine(wordLine);
            }

            Console.WriteLine($"\nВсего уникальных слов: {concordance.Count}");
        }
    }
}
