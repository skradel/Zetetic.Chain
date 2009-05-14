using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using System.Runtime.Remoting.Lifetime;
using System.Diagnostics;

namespace Zetetic.Chain
{
    public abstract class Command : MarshalByRefObject, ICommand
    {
        public abstract CommandResult Execute(IContext ctx);

        public bool? ShouldTerminateChain { get; set; }

        public string Name {  get; set; }

        public override string ToString()
        {
            return this.GetType().FullName + ", MBRO, name: " + (this.Name ?? "_unnamed");
        }
    }
}
