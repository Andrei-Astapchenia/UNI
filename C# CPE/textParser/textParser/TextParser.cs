using System;
using System.Collections.Generic;
using System.Text;

namespace textParser
{
    public class TextParser
    {
        public Text ParseText(string file)
        {
            string fullText=File.ReadAllText(file);
            Text text = new Text();

            List<string> Strings=SplitIntoSentences(fullText);
            foreach (string str in Strings)
            {
                if (!string.IsNullOrWhiteSpace(str))
                {
                    Sentence sentence = ParseSentence(str.Trim());
                    text.Sentences.Add(sentence);
                }
            }
                return text;
        }
        private List<string> SplitIntoSentences(string text)
        {
            List<string> sentences = new List<string>();
            StringBuilder thisSentence = new StringBuilder();

            foreach (char c in text)
            {
                thisSentence.Append(c);
                if (c == '.' || c == '!' || c == '?')
                {
                    sentences.Add(thisSentence.ToString());
                    thisSentence.Clear();
                }
            }
            if (thisSentence.Length > 0)
            {
                sentences.Add(thisSentence.ToString());
            }
            return sentences;
        }
        private Sentence ParseSentence(string thisSentence)
        {
            Sentence sentence = new Sentence();
            StringBuilder thisWord = new StringBuilder();
            foreach (char c in thisSentence)
            {
                if (char.IsLetterOrDigit(c)) {
                    thisWord.Append(c);
                }
                else
                    {
                    if (thisWord.Length > 0) 
                    {
                        sentence.Tokens.Add(new Word(thisWord.ToString()));
                        thisWord.Clear();
                    }
                    if (char.IsWhiteSpace(c))
                    {
                        if (sentence.Tokens.Count > 0) 
                        {
                            sentence.Tokens[sentence.Tokens.Count - 1].Value += " ";
                        }
                    }
                    if (char.IsPunctuation(c)) {
                        sentence.Tokens.Add(new Punctuation(c.ToString()));
                    }
                }
            }
            if (thisWord.Length > 0)
            {
                sentence.Tokens.Add(new Word(thisWord.ToString()));
            }
            return sentence;
        }
    }
}
    