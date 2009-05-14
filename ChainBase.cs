using System;
using System.Collections.Generic;
using System.Text;
using NLog;

namespace Zetetic.Chain
{
    public class ChainBase : IChain
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        protected List<ICommand> _cmds = new List<ICommand>();

        [System.Xml.Serialization.XmlIgnore]
        public bool Frozen { get; protected set; }

        internal ChainBase() { }

        #region IChain Members

        public string Name { get; set; }

        public virtual IEnumerable<ICommand> Commands()
        {
            foreach (ICommand cmd in _cmds)
                yield return cmd;
        }

        public virtual void Add(ICommand cmd)
        {
            if (this.Frozen)
                throw new ChainException("Chain is frozen to changes");

            _cmds.Add(cmd);
        }

        #endregion

        /// <summary>
        /// Fire the PostProcess method of every IFilter on which we called Execute
        /// </summary>
        /// <param name="filters"></param>
        /// <param name="ctx"></param>
        /// <param name="e"></param>
        /// <returns>True if postprocessing handled the error; otherwise false</returns>
        protected virtual bool InvokeFilters(Stack<IFilter> filters, IContext ctx, Exception e)
        {
            Exception ex = e;

            if (ex == null)
                logger.Debug("Calling {0} filters with no exception", filters.Count);
            else
                logger.DebugException("Calling " + filters.Count
                    + " filters with exception", ex);

            while (filters.Count > 0)
            {
                IFilter f = filters.Pop();
                try
                {
                    if (f.PostProcess(ctx, ex) == FilterResult.ExceptionHandled)
                    {
                        logger.Debug("Filter {0} handled exception {1}", f, ex);
                        ex = null;
                    }
                }
                catch (Exception filterErr)
                {
                    this.UnhandledFilterError(f, filterErr);
                }
            }

            return (ex == null);
        }

        protected virtual void UnhandledFilterError(IFilter filter, Exception filterErr)
        {
            logger.ErrorException("Filter " + filter + " threw exception", filterErr);
        }

        #region ICommand Members

        public virtual CommandResult Execute(IContext ctx)
        {
            this.Frozen = true;

            Stack<IFilter> filters = new Stack<IFilter>();
            CommandResult res = CommandResult.Continue;

            try
            {
                foreach (ICommand cmd in _cmds)
                {
                    if (cmd is IFilter)
                        filters.Push((IFilter)cmd);

                    res = cmd.Execute(ctx);

                    if (res == CommandResult.Stop)
                        break;
                }
            }
            catch (Exception e)
            {
                if (!this.InvokeFilters(filters, ctx, e))
                    throw;

                return res;
            }

            this.InvokeFilters(filters, ctx, null);
            return res;
        }

        #endregion
    }
}
