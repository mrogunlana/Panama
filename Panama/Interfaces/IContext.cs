using System.Collections.Generic;
using System.Threading;

namespace Panama.Core.Interfaces
{
    public interface IContext
    {
        IList<IModel> Data { get; set; }
        CancellationToken Token { get; set; }
    }
}
