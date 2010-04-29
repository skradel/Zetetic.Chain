using System;
using System.Collections.Generic;
using System.Text;
using Zetetic.Chain;
using NLog;

namespace Zetetic.Chain.Security
{
    public abstract class PamCommand : Command
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        [ChainRequired]
        public string PamFlag { get; set; }

        protected CommandResult AproposResponse(PamContext ctx, bool thisResult)
        {
            logger.Debug("{0}, pam {1}, thisResult {2}, existing-permit {3}",
                this.GetType(), this.PamFlag, thisResult, ctx.Permit);

            switch (this.PamFlag.ToLowerInvariant())
            {
                case "requisite":

                    ctx.Permit = thisResult;
                    
                    if (!ctx.Permit)
                        ctx.MarkDenied();
                    
                    return thisResult ? CommandResult.Continue 
                        : CommandResult.Stop;
                    
                case "required":

                    ctx.Permit = thisResult;

                    if (!ctx.Permit)
                        ctx.MarkDenied();

                    return CommandResult.Continue;

                case "sufficient":

                    if (thisResult)
                        ctx.Permit = true;

                    return thisResult ? CommandResult.Stop
                        : CommandResult.Continue;

                case "optional":

                    return CommandResult.Continue;

                default:
                    throw new ApplicationException("Unknown PamFlag " + this.PamFlag);
            }
        }
    }
}
