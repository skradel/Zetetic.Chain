using System;
using System.Collections.Generic;
using System.Text;
using NLog;
using Spring.Context;
using Spring.Context.Support;

namespace Zetetic.Chain
{
    public class SpringCatalog : CatalogBase
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public IApplicationContext SpringContext { get; set; }

        public SpringCatalog() : base() 
        {
            this.SpringContext = ContextRegistry.GetContext();
        }

        public override ICommand this[string cmdName]
        {
            get
            {
                try
                {
                    return (ICommand)this.SpringContext.GetObject(cmdName, typeof(ICommand));
                }
                catch (Spring.Objects.Factory.NoSuchObjectDefinitionException)
                {
                    throw new NoSuchCommandException(this, cmdName);
                }
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public override IEnumerable<string> GetCommandNames()
        {
            return this.SpringContext.GetObjectNamesForType(typeof(ICommand), true, false);
        }
    }
}
