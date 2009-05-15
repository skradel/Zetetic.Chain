using System;
using System.Collections.Generic;
using System.Text;

namespace Zetetic.Chain.Generic
{
    public class RemoveCommand : Command
    {
        [ChainRequired]
        public string FromKey { get; set; }

        public override CommandResult Execute(IContext ctx)
        {
            ctx[this.FromKey] = null;

            return CommandResult.Continue;
        }
    }
}
