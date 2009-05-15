using System;
using System.Collections.Generic;
using System.Text;

namespace Zetetic.Chain
{
    /// <summary>
    /// This attribute is a hint to the serializer routines in Zetetic.Chain.Xml.* that
    /// an ICommand requires the property to be non-emptyat runtime.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class ChainRequiredAttribute : System.Attribute
    {
    }
}
