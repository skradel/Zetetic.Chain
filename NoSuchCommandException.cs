using System;
using System.Collections.Generic;
using System.Text;

namespace Zetetic.Chain
{
    public class NoSuchCommandException : System.Exception
    {
        public string CommandRequested { get; protected set; }
        public string CatalogDescription { get; protected set; }

        public NoSuchCommandException(ICatalog source, string commandRequested)
            : base("The requested command was not found in the catalog")
        {
            this.CommandRequested = commandRequested;
            this.CatalogDescription = source.GetDescription();
        }
    }
}
