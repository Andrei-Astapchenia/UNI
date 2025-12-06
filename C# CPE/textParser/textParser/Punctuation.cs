using System;
using System.Collections.Generic;
using System.Text;

namespace textParser
{
    [Serializable]
    public class Punctuation : Token
    {
        public Punctuation(string value) : base(value)
        {
        }
        public Punctuation() : base("") { }
    }
}
