using System;
using System.Collections.Generic;
using System.Text;
using Zetetic.Chain;

namespace Zetetic.Chain.Generic
{
    public class LookupCommand : Command, IFilter
    {
        public string CatalogKey { get; set; }
        public string CatalogFactory { get; set; }
        public string Catalog { get; set; }

        [ChainRequired]
        public string Command { get; set; }

        protected ICommand InnerCommand;

        protected virtual void ResolveInnerCommand(IContext ctx, out string catSource)
        {
            ICatalog catalog;

            if (!string.IsNullOrEmpty(this.CatalogKey))
            {
                catalog = (ICatalog)ctx[this.CatalogKey];
                catSource = "key:" + this.CatalogKey;
            }
            else
            {
                CatalogFactory factory = Zetetic.Chain.CatalogFactory.GetFactory(this.CatalogFactory);
                catalog = factory.GetCatalog(this.Catalog);
                catSource = "factory:" + this.CatalogFactory + "/" + this.Catalog;
            }

            this.InnerCommand = catalog[this.Command];
        }

        public override CommandResult Execute(IContext ctx)
        {
            string catSource;
            this.ResolveInnerCommand(ctx, out catSource);

            if (this.InnerCommand == null)
                throw new ChainException("Command '" + this.Command + "' was not found in catalog "
                    + catSource);

            return this.ExecuteInnerCommand(ctx);
        }

        protected virtual CommandResult ExecuteInnerCommand(IContext ctx)
        {
            return this.InnerCommand.Execute(ctx);
        }

        #region IFilter Members

        public virtual FilterResult PostProcess(IContext ctx, Exception ex)
        {
            if (this.InnerCommand != null && this.InnerCommand is IFilter)
            {
                return ((IFilter)this.InnerCommand).PostProcess(ctx, ex);
            }
            return ex == null ? FilterResult.ExceptionHandled : FilterResult.ExceptionRethrow;
        }

        #endregion
    }
}
