using System;
using System.Collections.Generic;
using System.Text;
using NLog;
using Zetetic.Chain;
using System.Security.Principal;
using System.Configuration;

namespace Zetetic.Chain.Security
{
    public class RbacProvider
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public ICatalog Catalog { get; set; }

        public RbacProvider(ICatalog catalog)
        {
            this.Catalog = catalog;
        }

        public RbacProvider(string catalogPath)
        {
            this.Catalog = string.IsNullOrEmpty(catalogPath) ? CatalogFactory.GetFactory().GetCatalog()
                : CatalogFactory.GetFactory("file").GetCatalog(catalogPath);
        }

        public RbacProvider() : this(ConfigurationManager.AppSettings["rbac-path"])
        {
        }

        protected virtual void OnResult(PamContext context, string operation, bool result, bool isNoActionResult)
        {
            if (isNoActionResult)
                logger.Warn("No command for '{0}'; return default {1}", operation, result);
        }

        public virtual bool OperationPermitted(PamContext context, IPrincipal principal, string operation, object target, bool allowIfNoRule)
        {
            if (this.Catalog == null)
                throw new ApplicationException("Catalog is undefined");

            if (principal == null)
                throw new ArgumentNullException("principal");

            if (operation == null)
                throw new ArgumentNullException("operation");

            var ctx = context ?? new PamContext(principal, operation, target);
            ctx["RawTarget"] = target;

            try
            {
                this.Catalog[operation].Execute(ctx);
            }
            catch (Zetetic.Chain.NoSuchCommandException)
            {
                this.OnResult(ctx, operation, allowIfNoRule, true);
                return allowIfNoRule;
            }

            this.OnResult(ctx, operation, ctx.Permit, false);
            return ctx.Permit;
        }
    }
}
