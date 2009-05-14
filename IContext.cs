using System;
using System.Collections.Generic;
using System.Text;

namespace Zetetic.Chain
{
    public interface IContext
    {
        object this[string key] { get; set; }

        void Remove(string key);
    }
}
