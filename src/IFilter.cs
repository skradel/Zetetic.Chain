using System;
using System.Collections.Generic;
using System.Text;

namespace Zetetic.Chain
{
    public enum FilterResult { ExceptionHandled, ExceptionRethrow };

    public interface IFilter : ICommand
    {
        FilterResult PostProcess(IContext ctx, Exception ex);
    }
}
