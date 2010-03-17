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
    public class XmlCatalogAndMetadata
    {
        public XmlCatalog Catalog { get; protected set; }

        public readonly System.Collections.Hashtable Metadata = new System.Collections.Hashtable();

        public XmlCatalogAndMetadata(XmlCatalog catalog)
        {
            this.Catalog = catalog;
        }
    }

    public class CatalogFactory
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        private static CatalogFactory factoryInstance, fileFactoryInstance;
        private static readonly object factoryInstanceLock = new object();

        private readonly object _dflock = new object();
        private ICatalog _defaultCatalog;

        protected Dictionary<string, XmlCatalogAndMetadata> LoadedCatalogs
            = new Dictionary<string, XmlCatalogAndMetadata>();

        public static CatalogFactory GetFactory()
        {
            return CatalogFactory.GetFactory(null);
        }

        public static CatalogFactory GetFactory(string uri)
        {
            if ("FILE".Equals(uri, StringComparison.InvariantCultureIgnoreCase))
            {
                if (fileFactoryInstance == null)
                {
                    using (new ShortLock(factoryInstanceLock, TimeSpan.FromSeconds(5)))
                    {
                        if (fileFactoryInstance == null)
                            fileFactoryInstance = new FileBasedCatalogFactory();
                    }
                }
                return fileFactoryInstance;
            }
            else
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
        }

        internal CatalogFactory() { }

        public void ClearCatalogState()
        {
            logger.Debug("Clearing all in-memory catalog state");

            using (new ShortLock(LoadedCatalogs))
                LoadedCatalogs.Clear();

            using (new ShortLock(_dflock))
                _defaultCatalog = null;
        }

        protected XmlCatalogAndMetadata LoadXmlCatalog(string uri)
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

            XmlCatalog cat;

            using (System.Xml.XmlReader reader = System.Xml.XmlReader.Create(uri))
                cat = (XmlCatalog)ser.Deserialize(reader, devents);

            cat.LoadedFrom = uri;

            XmlCatalogAndMetadata xmd = new XmlCatalogAndMetadata(cat);
            xmd.Metadata.Add("uri", uri);

            return xmd;
        }

        public ICatalog GetCatalog()
        {
            return GetCatalog(ConfigurationManager.AppSettings["zetetic.chain.defaultcatalog"]);
        }

        public virtual ICatalog GetCatalog(string uri)
        {
            if (string.IsNullOrEmpty(uri) || "spring".Equals(uri, StringComparison.InvariantCultureIgnoreCase))
            {
                bool isNew = false;
                lock (_dflock)
                {
                    if (_defaultCatalog == null)
                    {
                        _defaultCatalog = new SpringCatalog();
                        isNew = true;
                    }
                }
                logger.Trace("Fetching default SpringCatalog (new: {0})", isNew);
                return _defaultCatalog;
            }
            else
            {
                XmlCatalogAndMetadata cat = null;

                using (new ShortLock(LoadedCatalogs))
                    if (LoadedCatalogs.ContainsKey(uri))
                        cat = LoadedCatalogs[uri];

                if (cat == null)
                {
                    cat = LoadXmlCatalog(uri);

                    using (new ShortLock(LoadedCatalogs))
                        LoadedCatalogs[uri] = cat;

                    logger.Info("Created and stored XmlCatalog from {0}", uri);
                }
                else
                {
                    logger.Debug("Reuse existing catalog from {0}", uri);
                }

                return cat.Catalog;
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
