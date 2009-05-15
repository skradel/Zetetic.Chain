using System;
using System.Collections.Generic;
using System.Text;
using NLog;

namespace Zetetic.Chain.Generic
{
    public class CopyCommand : Command
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        [ChainRequired]
        public string FromKey { get; set; }

        [ChainRequired]
        public string ToKey { get; set; }

        public bool SkipIfNull { get; set; }

        public override CommandResult Execute(IContext ctx)
        {
            logger.Debug("Invoke FromKey {0}, ToKey {1}", this.FromKey, this.ToKey);

            object o = ctx[this.FromKey];

            if (o == null)
            {
                if (!this.SkipIfNull)
                {
                    logger.Debug("Delete {0}", this.ToKey);
                    ctx.Remove(this.ToKey);
                }
            }
            else
            {
                logger.Debug("Set key {0} = {1}", this.ToKey, o);
                ctx[this.ToKey] = o;
            }
            return CommandResult.Continue;
        }
    }
}
