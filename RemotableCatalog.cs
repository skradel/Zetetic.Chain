using System;
using System.Collections.Generic;
using System.Text;
using System.Configuration;
using NLog;

namespace Zetetic.Chain
{
    /// <summary>
    /// Utility class for Windows services, etc., that may wish to host a single instance of
    /// a catalog.  The application setting 'zetetic.chain.remotablecatalog.uri' should specify
    /// the URI of the XML catalog configuration to load.
    /// </summary>
    public class RemotableCatalog : MarshalByRefObject, ICatalog
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        private ICatalog _realCatalog = null;

        public RemotableCatalog()
        {
            string loadFrom = ConfigurationManager.AppSettings["zetetic.chain.remotablecatalog.uri"];
            logger.Info("ctor called; load from {0}", loadFrom);
            try
            {
                this._realCatalog = CatalogFactory.GetFactory().GetCatalog(loadFrom);
                logger.Info("Ready: {0}", this._realCatalog.GetDescription());
            }
            catch (Exception ex)
            {
                logger.FatalException("RemotableCatalog constructor failed: " + ex.Message, ex);
                throw;
            }
        }

        #region ICatalog Members

        public ICommand this[string key]
        {
            get
            {
                return this._realCatalog[key];
            }
            set
            {
                this._realCatalog[key] = value;
            }
        }

        public IEnumerable<string> GetCommandNames()
        {
            return _realCatalog.GetCommandNames();
        }

        public void Add(ICommand cmd)
        {
            _realCatalog[cmd.Name] = cmd;
        }

        public string GetDescription()
        {
            return "RemoteCatalog wrapping " + _realCatalog.GetDescription();
        }

        #endregion
    }
}
