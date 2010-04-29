using System;
using System.Collections.Generic;
using System.Text;

namespace Zetetic.Chain.Security
{
    public class IsAuthenticatedCommand : PamCommand
    {
        public override Zetetic.Chain.CommandResult Execute(Zetetic.Chain.IContext ctx)
        {
            PamContext c = (PamContext)ctx;

            if (c.Principal != null && c.Principal.Identity.IsAuthenticated)
            {
                c.Permit = true;
                return this.AproposResponse(c, true);
            }
            else
            {
                return this.AproposResponse(c, false);
            }
        }
    }
}
