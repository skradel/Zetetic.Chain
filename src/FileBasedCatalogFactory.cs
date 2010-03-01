using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Zetetic.Chain.Xml;

namespace Zetetic.Chain
{
    public class FileBasedCatalogFactory : CatalogFactory
    {
        internal FileBasedCatalogFactory() : base() { }

        public override ICatalog GetCatalog(string uri)
        {
            FileInfo fi = new FileInfo(uri);

            if (!fi.Exists)
                throw new ChainException("File " + uri + " does not exist");

            XmlCatalogAndMetadata cat = null;

            lock (this.LoadedCatalogs)
            {
                if (this.LoadedCatalogs.TryGetValue(uri, out cat))
                {
                    if (fi.LastWriteTime.Ticks > ((DateTime)cat.Metadata["LastMod"]).Ticks)
                    {
                        cat = this.LoadXmlCatalog(uri);
                        cat.Metadata.Add("LastMod", fi.LastWriteTime);
                        this.LoadedCatalogs[uri] = cat;
                    }
                }
                else
                {
                    cat = this.LoadXmlCatalog(uri);
                    cat.Metadata.Add("LastMod", fi.LastWriteTime);
                    this.LoadedCatalogs[uri] = cat;
                }
            }

            return cat.Catalog;
        }
    }
}
