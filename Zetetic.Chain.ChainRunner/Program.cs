using System;
using System.Collections.Generic;
using System.Text;
using Zetetic.Chain;

namespace Zetetic.Chain
{
    public class ChainRunner
    {
        static void Main(string[] args)
        {
            string catalogsrc = "catalog.xml", command = "default";

            for (int i = 0; i < args.Length; i++)
            {
                switch (args[i])
                {
                    case "-c":
                        catalogsrc = args[++i];
                        break;

                    case "-x":
                        command = args[++i];
                        break;
                }
            }

            try
            {
                ICatalog catalog = CatalogFactory.GetFactory().GetCatalog(catalogsrc);
                IContext ctx = new ContextBase();

                catalog[command].Execute(ctx);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.Message + " :: " + ex.StackTrace);
                Environment.ExitCode = 1;
            }
        }
    }
}
