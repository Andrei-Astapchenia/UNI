using System;
using System.Collections.Generic;
using System.Text;

namespace textParser
{
    public class Concordance
    {
        public string Word { get; set; }
        public int Count { get; set; }
        public SortedSet<int> SentenceNumbers { get; set; } 

        public Concordance(string word)
        {
            Word = word;
            Count = 0;
            SentenceNumbers = new SortedSet<int>();
        }
    }
}
