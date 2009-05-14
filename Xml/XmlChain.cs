using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using NLog;

namespace Zetetic.Chain.Xml
{
    public class XmlChain : XmlConfig
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public XmlChain() { }

        public XmlChain(string cmdName)
        {
            this.Name = cmdName;
        }

        [XmlElement("command", Type = typeof(XmlCommand))]
        [XmlElement("chain", Type = typeof(XmlChain))]
        public List<XmlConfig> Commands = new List<XmlConfig>();

        public override ICommand ResolveInternals()
        {
            logger.Debug("Resolving chain {0} with {1} direct children", 
                this.Name, this.Commands.Count);

            IChain chain = ChainFactory.GetFactory().CreateChain();
            chain.Name = this.Name;

            foreach (XmlConfig cfg in Commands)
                chain.Add(cfg.ResolveInternals());

            logger.Trace("Chain {0} finished resolving", this.Name);
            return this.DeserializeProperties(chain);
        }
    }
}
