using System;
using System.Collections.Generic;
using System.Text;

namespace Zetetic.Chain
{
    public class ContextBase : MarshalByRefObject, IContext
    {
        protected Dictionary<string, object> BaseStorage = new Dictionary<string, object>();

        #region IContext Members

        public virtual object this[string key]
        {
            get
            {
                if (BaseStorage.ContainsKey(key))
                {
                    return BaseStorage[key];
                }
                return null;
            }
            set
            {
                BaseStorage[key] = value;
            }
        }


        public virtual void Remove(string key)
        {
            if (BaseStorage.ContainsKey(key))
            {
                BaseStorage.Remove(key);
            }
        }

        #endregion
    }
}
