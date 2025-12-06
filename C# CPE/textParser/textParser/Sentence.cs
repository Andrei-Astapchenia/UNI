using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace textParser
{
    [Serializable]
    public class Sentence
    {
        [XmlArray("Tokens")]
        [XmlArrayItem("Token")]
        public List<Token> Tokens { get; set; } =new List<Token>();

        public string GetSentence()
        {
            StringBuilder sentence = new StringBuilder();
            foreach (var token in Tokens) 
            {
                sentence.Append(token.Value);
            }
            return sentence.ToString();
        }
        public int CountOfWordsInSentense
        {
            get 
            { int count = 0;
                foreach (var token in Tokens)
                {
                    if (token is Word)
                    {
                        count++;
                    }
                }
                return count;
            }
        }
    }
}
