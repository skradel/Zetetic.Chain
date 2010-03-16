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

        public IApplicationContext SpringContext { get; set; }

        public SpringCatalog() : base() 
        {
            this.LoadedFrom = "Spring:ContextRegistry";
            this.SpringContext = ContextRegistry.GetContext();
        }

        protected override void CheckDictionary()
        {
            if (this.BaseStorage.Count == 0 && __XmlCommands != null && __XmlCommands.Count > 0)
            {
               
                logger.Debug("Resolving {0} XmlConfigs", __XmlCommands.Count);

                foreach (XmlConfig cfg in __XmlCommands)
                {
                    this.BaseStorage[cfg.Name] = (ICommand)this.SpringContext
                        .GetObject(cfg.Name, typeof(ICommand));
                }
            }
        }
    }
}
