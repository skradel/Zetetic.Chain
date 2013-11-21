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
            string catalogsrc = "catalog.xml", command = "default", logfile = null, contextType = null;

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

                    case "-log":
                    case "-f":
                        logfile = args[++i];
                        break;

                    case "-ctxtype":
                        contextType = args[++i];
                        break;
                }
            }

            try
            {
                ICatalog catalog = CatalogFactory.GetFactory().GetCatalog(catalogsrc);
                IContext ctx;

                if (string.IsNullOrEmpty(contextType))
                {
                    ctx = new ContextBase();
                }
                else
                {
                    ctx = (IContext)Activator.CreateInstance(Type.GetType(contextType, true));
                }

                catalog[command].Execute(ctx);
            }
            catch (Exception ex)
            {
                if (!string.IsNullOrEmpty(logfile))
                    try
                    {
                        System.IO.File.AppendAllText(logfile, ex.Message + " :: " + ex.StackTrace, Encoding.UTF8);
                    }
                    catch (Exception) { }

                Console.Error.WriteLine(ex.Message + " :: " + ex.StackTrace);
                Environment.ExitCode = 1;
            }
        }
    }
}
