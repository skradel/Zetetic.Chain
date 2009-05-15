using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using NLog;

namespace Zetetic.Chain.Generic
{
    /// <summary>
    /// Lookup and execute a method other than 'Execute' in an ICommand implementation.
    /// </summary>
    public class DispatchLookupCommand : LookupCommand, IFilter
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        [ChainRequired]
        public string DispatchMethod { get; set; }

        protected override void ResolveInnerCommand(IContext ctx, out string catSource)
        {
            if (this.Command == "_self")
            {
                catSource = "_self";
                this.InnerCommand = this;
            }
            else
            {
                base.ResolveInnerCommand(ctx, out catSource);
            }
        }

        protected override CommandResult ExecuteInnerCommand(IContext ctx)
        {
            MethodInfo meth = this.InnerCommand.GetType().GetMethod(this.DispatchMethod);
            if (meth == null)
            {
                this.InnerCommand = null;
                throw new DispatchTargetException("Dispatch method " + this.DispatchMethod 
                    + " not found on target command");
            }

            ParameterInfo[] pi = meth.GetParameters();
            if (pi.Length == 1 && pi[0].ParameterType.IsAssignableFrom(typeof(IContext)))
            {
                logger.Trace("Method {0} of {1} takes appropriate parameters", meth.Name,
                    this.InnerCommand);
            }
            else
            {
                this.InnerCommand = null;
                throw new DispatchTargetException("Dispatch method " + this.DispatchMethod 
                    + " must accept one IContext parameter");
            }

            if (meth.DeclaringType == typeof(DispatchLookupCommand))
            {
                this.InnerCommand = null;
                throw new DispatchTargetException("Loop condition; cannot dispatch to a method declared on DispatchLookupCommand");
            }

            object o = meth.Invoke(this.InnerCommand, new object[]{ctx});
            logger.Debug("Dispatched invoke of {0} returns {1}", meth.Name, o);

            return this.InterpretResult(o);
        }

        protected virtual CommandResult InterpretResult(object o)
        {
            if (o is bool)
            {
                return (bool)o ? CommandResult.Stop : CommandResult.Continue;
            }
            return (CommandResult)o;
        }
    }
}
