using System;
using System.Collections.Generic;
using System.Text;

namespace Zetetic.Chain.Xml
{
    public class SpringChain : ChainBase
    {
        public List<ICommand> Commands
        {
            get
            {
                return this._cmds;
            }
            set
            {
                this._cmds = value;
            }
        }

        public SpringChain() { }
    }
}
