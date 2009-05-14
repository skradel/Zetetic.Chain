using System;
using System.Collections.Generic;
using System.Text;

namespace Zetetic.Chain.Generic
{
    public class ContextNotRemoteableException : Exception
    {
        public ContextNotRemoteableException(string msg) : base(msg) { }
    }
}
