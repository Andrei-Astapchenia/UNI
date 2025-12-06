using System;
using System.Collections.Generic;
using System.Text;

namespace textParser
{
    [Serializable]
    public class Word : Token
    {
        public Word(string value):base(value) { }
        public Word() : base("") { }//xml constructor
    }
}
