using System;
using System.Collections.Generic;
using System.Text;

namespace Zetetic.Chain.Security
{
    public class SuccessCommand : PamCommand
    {
        public override Zetetic.Chain.CommandResult Execute(Zetetic.Chain.IContext ctx)
        {
            return this.AproposResponse((PamContext)ctx, true);
        }
    }
}
