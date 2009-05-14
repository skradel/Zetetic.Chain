using System;
using System.Collections.Generic;
using System.Text;

namespace Zetetic.Chain.Xml
{
    public class ChainXmlSerializationException : ChainException
    {
        public string PropertyName { get; protected set; }

        public ChainXmlSerializationException(string msg, string propertyName)
            : base(msg)
        {
            this.PropertyName = propertyName;
        }
    }
}
