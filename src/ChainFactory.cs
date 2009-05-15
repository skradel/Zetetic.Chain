using System;
using System.Collections.Generic;
using System.Text;
using System.Configuration;

namespace Zetetic.Chain
{
    public class ChainFactory
    {
        private static ChainFactory instance;
        private readonly static object instlock = new object();

        public static ChainFactory GetFactory()
        {
            if (instance == null) 
            {
                lock (instlock) 
                {
                    if (instance == null) 
                        instance = new ChainFactory();
                }
            }
            return instance;
        }

        private ChainFactory() { }

        public virtual IChain CreateChain() 
        {
            return CreateChain(ConfigurationManager.AppSettings["zetetic.chain.chaintype"]);
        }

        public virtual IChain CreateChain(string chainType)
        {
            if (string.IsNullOrEmpty(chainType))
            {
                return new ChainBase();
            }
            else
            {
                Type t = Type.GetType(chainType, true, true);

                if (t.IsSubclassOf(typeof(IChain)))
                    return (IChain)Activator.CreateInstance(t);

                throw new ChainException("Type " + t.FullName 
                    + " does not subclass IChain");
            }
        }
    }
}
