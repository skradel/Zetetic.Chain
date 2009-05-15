using System;
using System.Collections.Generic;
using System.Text;

namespace Zetetic.Chain
{
    public enum CommandResult { Continue, Stop };

    public interface ICommand
    {
        CommandResult Execute(IContext ctx);

        string Name { get; set; }
    }
}
