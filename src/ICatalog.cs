using System;
using System.Collections.Generic;
using System.Text;

namespace Zetetic.Chain
{
    public interface ICatalog
    {
        ICommand this[string key] { get; set; }

        IEnumerable<string> GetCommandNames();

        void Add(ICommand cmd);

        string GetDescription();
    }
}
