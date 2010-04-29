using System;
using System.Collections.Generic;
using System.Text;

namespace Zetetic.Chain
{
    public interface IChain : ICommand
    {
        void Add(ICommand cmd);

        IList<ICommand> Commands { get; set; }
    }
}
