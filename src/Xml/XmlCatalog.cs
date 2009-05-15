using System;
using System.Collections.Generic;
using System.Text;
using Zetetic.Chain;
using System.Xml.Serialization;
using System.Collections.ObjectModel;
using NLog;

namespace Zetetic.Chain.Xml
{
    [XmlRoot("catalog", Namespace = "http://zetetic.net/schemas/chain/catalog.xsd")]
    public class XmlCatalog : CatalogBase, ICatalog
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        [XmlIgnore]
        public string LoadedFrom { get; internal set; }

        public XmlCatalog() : base() 
        {
            this.CommandAdded += new CatalogEventHandler(XmlCatalog_CommandAdded);
            this.CommandRemoved += new CatalogEventHandler(XmlCatalog_CommandRemoved);
        }

        public override string GetDescription()
        {
            return base.GetDescription() + " loaded from " + this.LoadedFrom;
        }

        void XmlCatalog_CommandRemoved(object sender, CatalogChangedEventArgs e)
        {
            logger.Info("Removed command {0} (replacement coming? {1})",
                e.CommandName, e.IsBeingReplaced);
        }

        void XmlCatalog_CommandAdded(object sender, CatalogChangedEventArgs e)
        {
            logger.Info("Added command {0}", e.CommandName);
        }

        /// <summary>
        /// Vivify the array of serializable XmlCommand and XmlChain into the real thing
        /// within the (unXmlSerializable) dictionary
        /// </summary>
        protected virtual void CheckDictionary()
        {
            if (this.BaseStorage.Count == 0 && __XmlCommands != null && __XmlCommands.Count > 0)
            {
                logger.Debug("Resolving {0} XmlConfigs", __XmlCommands.Count);

                foreach (XmlConfig cfg in __XmlCommands)
                {
                    this.BaseStorage[cfg.Name] = cfg.ResolveInternals();
                }
            }
        }

        /// <summary>
        /// Serializable storage of IChain and ICommand configurations.
        /// Do not access this elsewhere -- visibility is public only to
        /// satisfy XML serialization.
        /// </summary>
        [XmlElement("command", Type = typeof(XmlCommand))]
        [XmlElement("chain", Type = typeof(XmlChain))]
        public List<XmlConfig> __XmlCommands = new List<XmlConfig>();

        /// <summary>
        /// Delete an item the internal serializable list of commands by name
        /// </summary>
        /// <param name="cmdName"></param>
        protected void RemoveInternalByName(string cmdName)
        {
            for (int i = 0; i < __XmlCommands.Count; i++)
            {
                if (cmdName == __XmlCommands[i].Name)
                {
                    __XmlCommands.RemoveAt(i);
                    return;
                }
            }
        }

        #region ICatalog Members

        public override ICommand this[string cmdName]
        {
            set
            {
                this.CheckDictionary();

                if (value == null)
                {
                    this.RemoveInternalByName(cmdName);
                }
                else
                {
                    if (this.BaseStorage.ContainsKey(cmdName))
                    {
                        // Replace command in serializable list
                        bool replaced = false;
                        for (int i = 0; !replaced && i < __XmlCommands.Count; i++)
                        {
                            if (cmdName == __XmlCommands[i].Name)
                            {
                                __XmlCommands[i] = XmlCommand.CreateFromICommand(value);
                                replaced = true;
                            }
                        }
                        if (!replaced) __XmlCommands.Add(XmlCommand.CreateFromICommand(value));
                    }
                    else
                    {
                        __XmlCommands.Add(XmlCommand.CreateFromICommand(value));
                    }
                }

                base[cmdName] = value;
            }

            /// <summary>
            /// Locate a command by name
            /// </summary>
            /// <param name="cmdName"></param>
            /// <returns></returns>
            get
            {
                this.CheckDictionary();

                return base[cmdName];
            }
        }

        /// <summary>
        /// Enumerate all command names
        /// </summary>
        /// <returns></returns>
        public override IEnumerable<string> GetCommandNames()
        {
            this.CheckDictionary();

            return base.GetCommandNames();
        }

        #endregion
    }
}
