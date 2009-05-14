using System;
using System.Collections.Generic;
using System.Text;

namespace Zetetic.Chain.Generic
{
    public class DispatchTargetException : Exception
    {
        public DispatchTargetException(string msg) : base(msg) { }
    }
}
