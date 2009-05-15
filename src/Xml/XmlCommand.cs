using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using NLog;

namespace Zetetic.Chain.Xml
{
    public class XmlCommand : XmlConfig
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public XmlCommand() { }

        public XmlCommand(string cmdName, string typeName)
        {
            this.Name = cmdName;
            this.TypeName = typeName;
        }

        public XmlCommand(string cmdName, Type t) : this(cmdName, t.AssemblyQualifiedName) { }

        [XmlAttribute("typeName")]
        public string TypeName { get; set; }

        public override ICommand ResolveInternals()
        {
            logger.Debug("Command {0} resolving type {1}", this.Name, this.TypeName);

            Type t = Type.GetType(this.TypeName, true);

            return this.DeserializeProperties((ICommand)Activator.CreateInstance(t));
        }
    }
}
