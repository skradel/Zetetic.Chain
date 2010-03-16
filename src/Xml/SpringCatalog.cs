using System;
using System.Collections.Generic;
using System.Text;
using NLog;
using Spring.Context;
using Spring.Context.Support;

namespace Zetetic.Chain.Xml
{
    public class SpringCatalog : XmlCatalog
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        protected override void CheckDictionary()
        {
            if (this.BaseStorage.Count == 0 && __XmlCommands != null && __XmlCommands.Count > 0)
            {
                IApplicationContext ctx = ContextRegistry.GetContext();

                logger.Debug("Resolving {0} XmlConfigs", __XmlCommands.Count);

                foreach (XmlConfig cfg in __XmlCommands)
                {
                    this.BaseStorage[cfg.Name] = (ICommand)ctx.GetObject(cfg.Name, typeof(ICommand));
                }
            }
        }
    }
}
