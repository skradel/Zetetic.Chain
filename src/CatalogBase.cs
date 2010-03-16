using System;
using System.Collections.Generic;
using System.Text;
using NLog;

namespace Zetetic.Chain
{
    public delegate void CatalogEventHandler(object sender, CatalogChangedEventArgs e);

    public class CatalogBase : MarshalByRefObject, ICatalog
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Subclasses should only access BaseStorage when there is a compelling reason
        /// to avoid a loop, such as overriding the string indexer proprety.
        /// </summary>
        protected Dictionary<string, ICommand> BaseStorage = new Dictionary<string, ICommand>();

        #region Events

        public event CatalogEventHandler CommandAdded;
        public event CatalogEventHandler CommandRemoved;

        #endregion

        #region ICatalog Members

        public virtual string GetDescription()
        {
            return this.GetType().FullName;
        }

        public virtual ICommand this[string cmdName]
        {
            get
            {
                if (BaseStorage.ContainsKey(cmdName))     
                    return BaseStorage[cmdName];
                
                throw new NoSuchCommandException(this, cmdName);
            }

            set
            {
                if (value == null)
                {
                    if (this.BaseStorage.ContainsKey(cmdName))
                    {
                        ICommand cmd = this.BaseStorage[cmdName];
                        this.BaseStorage.Remove(cmdName);

                        if (CommandRemoved != null)
                            CommandRemoved(this, new CatalogChangedEventArgs(cmdName, cmd, false));                     
                    }
                }
                else
                {
                    if (CommandRemoved != null && this.BaseStorage.ContainsKey(cmdName))            
                        CommandRemoved(this, new CatalogChangedEventArgs(cmdName, BaseStorage[cmdName], true));
                    
                    BaseStorage[value.Name] = value;

                    if (CommandAdded != null)
                        CommandAdded(this, new CatalogChangedEventArgs(cmdName, value, false)); 
                }
            }
        }

        public virtual IEnumerable<string> GetCommandNames()
        {
            foreach (string key in BaseStorage.Keys)
                yield return key;
        }

        public virtual void Add(ICommand cmd)
        {
            this[cmd.Name] = cmd;
        }

        #endregion
    }
}
