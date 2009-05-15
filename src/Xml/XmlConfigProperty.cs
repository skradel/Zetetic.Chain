using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace Zetetic.Chain.Xml
{
    public struct XmlConfigProperty
    {
        public XmlConfigProperty(string key, string value)
        {
            this.Key = key;
            this.Value = value;
        }

        [XmlAttribute("key")]
        public string Key;

        [XmlAttribute("value")]
        public string Value;
    }
}
