using System;
using System.Collections.Generic;
using System.Text;

namespace Zetetic.Chain
{
    public enum CatalogChangeType { Add, Replace, Remove };

    public class CatalogChangedEventArgs : EventArgs
    {
        public string CommandName { get; protected set; }
        public ICommand Command { get; protected set; }
        public bool IsBeingReplaced { get; protected set; }

        public CatalogChangedEventArgs(string cmdName, ICommand cmd, bool isBeingReplaced)
        {
            this.CommandName = cmdName;
            this.Command = cmd;
            this.IsBeingReplaced = IsBeingReplaced;
        }
    }
}
