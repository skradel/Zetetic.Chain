using System;
using System.Collections.Generic;
using System.Text;

namespace Zetetic.Chain
{
    public class ChainException : System.Exception
    {
        public ChainException(string msg) : base(msg) { }
    }
}
