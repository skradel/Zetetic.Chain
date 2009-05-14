using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using System.Configuration;
using NLog;
using System.Threading;
using Zetetic.Chain.Xml;

namespace Zetetic.Chain
{
    public class CatalogFactory
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        private static CatalogFactory factoryInstance;
        private static readonly object factoryInstanceLock = new object();

        private readonly object _dflock = new object();
        private ICatalog _defaultCatalog;

        private Dictionary<string, Zetetic.Chain.Xml.XmlCatalog> _xmlCatalogs
            = new Dictionary<string, Zetetic.Chain.Xml.XmlCatalog>();

        public static CatalogFactory GetFactory()
        {
            return CatalogFactory.GetFactory(null);
        }

        public static CatalogFactory GetFactory(string uri)
        {
            if (factoryInstance == null)
            {
                using (new ShortLock(factoryInstanceLock, TimeSpan.FromSeconds(5)))
                {
                    if (factoryInstance == null)
                        factoryInstance = new CatalogFactory();
                }
            }

            return factoryInstance;
        }

        private CatalogFactory() { }

        public void ClearCatalogState()
        {
            logger.Debug("Clearing all in-memory catalog state");

            using (new ShortLock(_xmlCatalogs))
                _xmlCatalogs.Clear();

            using (new ShortLock(_dflock))
                _defaultCatalog = null;

        }

        public ICatalog GetCatalog()
        {
            return GetCatalog(ConfigurationManager.AppSettings["zetetic.chain.defaultcatalog"]);
        }

        public ICatalog GetCatalog(string uri)
        {
            if (string.IsNullOrEmpty(uri))
            {
                bool isNew = false;
                lock (_dflock)
                {
                    if (_defaultCatalog == null)
                    {
                        _defaultCatalog = new CatalogBase();
                        isNew = true;
                    }
                }
                logger.Debug("Fetching default in-memory catalog (new: {0})", isNew);
                return _defaultCatalog;
            }
            else
            {
                XmlCatalog cat = null;

                using (new ShortLock(_xmlCatalogs))
                    if (_xmlCatalogs.ContainsKey(uri))
                        cat = _xmlCatalogs[uri];

                if (cat == null)
                {
                    XmlSerializer ser = new XmlSerializer(typeof(XmlCatalog));

                    XmlDeserializationEvents devents = new XmlDeserializationEvents();
                    devents.OnUnknownAttribute = delegate(object sender, XmlAttributeEventArgs e)
                    {
                        logger.Warn("Unknown attribute {0}", e.Attr.Name);
                    };

                    devents.OnUnknownElement = delegate(object sender, XmlElementEventArgs e)
                    {
                        logger.Warn("Unknown element {0}", e.Element.Name);
                    };

                    using (System.Xml.XmlReader reader = System.Xml.XmlReader.Create(uri))
                        cat = (XmlCatalog)ser.Deserialize(reader, devents);

                    cat.LoadedFrom = uri;

                    using (new ShortLock(_xmlCatalogs))
                        _xmlCatalogs[uri] = cat;

                    logger.Info("Created and stored XmlCatalog from {0}", uri);
                }
                else
                {
                    logger.Debug("Reuse existing catalog from {0}", uri);
                }

                return cat;
            }
        }
    }

    internal class LockFailedException : Exception
    {
        internal LockFailedException() : base() { }
    }

    internal struct ShortLock : IDisposable
    {
        private object _o;

        internal ShortLock(object toLock, TimeSpan ts)
        {
            _o = toLock;
            if (!Monitor.TryEnter(toLock, ts))
                throw new LockFailedException();
        }

        internal ShortLock(object toLock) : this(toLock, TimeSpan.FromSeconds(1)) { }

        public void Dispose()
        {
            Monitor.Exit(_o);
        }
    }
}
