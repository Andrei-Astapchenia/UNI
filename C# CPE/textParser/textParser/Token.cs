using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace textParser
{
    [Serializable]
    [XmlInclude(typeof(Word))]
    [XmlInclude(typeof(Punctuation))]
    public abstract class Token
    {
        [XmlElement("Value")]
        public string Value {  get; set; }
        protected Token(string value) {
            Value = value;
        }
        protected Token() { }
        public override string ToString()
        {
            return Value;
        }
    }
}
