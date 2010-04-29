using System;
using System.Collections.Generic;
using System.Text;
using Zetetic.Chain;
using System.Security.Principal;
using System.Configuration;
using NLog;

namespace Zetetic.Chain.Security
{
    public class AllowRolesCommand : PamCommand
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        [ChainRequired]
        public string Roles { get; set; }

        public override CommandResult Execute(IContext ctx)
        {
            PamContext rc = (PamContext)ctx;

            if (!rc.Principal.Identity.IsAuthenticated)
                return this.AproposResponse(rc, false);

            char sep = this.Roles.Contains("!") ? '!' : ',';

            foreach (string s in this.Roles.Split(sep))
            {
                try
                {
                    if (s.Contains("=") && rc.Principal.Identity is WindowsIdentity)
                    {
                        logger.Debug("Not testing DN-style role {0} for a WindowsIdentity", s);
                    }
                    else if (rc.Principal.IsInRole(s))
                    {
                        logger.Trace("{0} is in role {1}", rc.Principal.Identity.Name, s);

                        return this.AproposResponse(rc, true);
                    }
                    else
                    {
                        logger.Trace("{0} not in role {1}", rc.Principal.Identity.Name, s);
                    }
                }
                catch (Exception ex)
                {
                    logger.ErrorException("Failure testing role " + s
                        + ": " + ex.Message, ex);
                }
            }

            return this.AproposResponse(rc, false);
        }
    }
}
