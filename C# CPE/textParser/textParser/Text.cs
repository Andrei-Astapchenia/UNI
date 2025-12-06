using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace textParser
{
    [Serializable]
    [XmlRoot("Text")]
    public class Text
    {
        [XmlArray("Sentences")]
        [XmlArrayItem("Sentence")]
        public List<Sentence> Sentences {  get; set; }= new List<Sentence>();
    }
}
