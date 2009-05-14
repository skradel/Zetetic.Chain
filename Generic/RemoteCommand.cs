using System;
using System.Collections.Generic;
using System.Text;
using NLog;
using System.Threading;
using System.Runtime.Remoting;

namespace Zetetic.Chain.Generic
{

    public class RemoteCommand : Command
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        protected static bool _firstCall = true;
        protected static readonly object _fcLock = new object();

        [ChainRequired]
        public string CatalogUrl { get; set; }

        [ChainRequired]
        public string Command { get; set; }

        [ChainRequired]
        public string RemotingConfig { get; set; }

        protected virtual void ConfigureRemoting(IContext ctx)
        {
            logger.Debug("Configure remoting from {0}", this.RemotingConfig);
            System.Runtime.Remoting.RemotingConfiguration.Configure(this.RemotingConfig, false);
        }

        public override CommandResult Execute(IContext ctx)
        {
            if (!(ctx is MarshalByRefObject))
            {
                throw new ContextNotRemoteableException("IContext "
                    + ctx.GetType() + " is not remoteable (subclass of MarshalByRefObject)");
            }

            // Statically set up remoting configuration before first call
            if (Monitor.TryEnter(_fcLock, TimeSpan.FromSeconds(15)))
            {
                try
                {
                    if (_firstCall)
                    {
                        this.ConfigureRemoting(ctx);
                        _firstCall = false;
                    }
                }
                finally
                {
                    Monitor.Exit(_fcLock);
                }
            }
            else
            {
                logger.Error("Unable to obtain exclusive lock on first-call state");
            }

            logger.Debug("Activating remote catalog at {0}", this.CatalogUrl);

            ICatalog catalog = (ICatalog)Activator.GetObject(typeof(ICatalog), this.CatalogUrl);

            logger.Debug("Got catalog {0}, invoking {1}", catalog.GetDescription(), this.Command);

            try
            {
                return catalog[this.Command].Execute(ctx);
            }
            finally
            {
                logger.Debug("Done");
            }
        }
    }
}
