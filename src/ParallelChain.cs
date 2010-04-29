using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using NLog;

namespace Zetetic.Chain
{
    /// <summary>
    /// Execute all commands in the chain asynchronously.  Note that the "Stop" behavior
    /// is irrelevant here - this method will always return Continue or throw an exception.
    /// </summary>
    public class ParallelChain : ChainBase
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public override CommandResult Execute(IContext ctx)
        {
            this.Frozen = true;

            ManualResetEvent[] waiters = new ManualResetEvent[this.Commands.Count];
            ParallelCommand[] cmds = new ParallelCommand[this.Commands.Count];
            Stack<IFilter> filters = new Stack<IFilter>();

            int i = -1;
            foreach (ICommand cmd in this.Commands)
            {
                i++;

                waiters[i] = new ManualResetEvent(false);
                cmds[i] = new ParallelCommand(cmd, waiters[i]);
                ThreadPool.QueueUserWorkItem(cmds[i].Execute, ctx);

                if (cmd is IFilter)
                    filters.Push((IFilter)cmd);
            }

            logger.Debug("Waiting");

            foreach (ManualResetEvent m in waiters)
                m.WaitOne();

            logger.Debug("Got all results");

            Exception ex = null;

            foreach (ParallelCommand c in cmds)
            {
                try
                {
                    c.GetResult();
                }
                catch (Exception e)
                {
                    logger.Debug("Collected the first exception to pass to filters");
                    ex = e;
                    break;
                }
            }

            if (!this.InvokeFilters(filters, ctx, ex))
                throw ex;

            return CommandResult.Continue;
        }

        internal class ParallelCommand
        {
            private ICommand _realCommand;
            private Exception _ex;
            private CommandResult _result;
            private ManualResetEvent _done;

            internal ParallelCommand(ICommand cmd, ManualResetEvent done)
            {
                _realCommand = cmd;
                _done = done;
            }

            internal CommandResult GetResult()
            {
                if (_ex != null)
                    throw _ex;

                return _result;
            }

            internal void Execute(object threadContext)
            {
                try
                {
                    _result = _realCommand.Execute((IContext)threadContext);
                }
                catch (Exception ex)
                {
                    logger.ErrorException("Parallelized error: " + ex.Message, ex);
                    _ex = ex;
                }
                finally
                {
                    _done.Set();
                }
            }
        }
    }
}
