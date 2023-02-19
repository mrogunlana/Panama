using System.Collections.Generic;
using System.Threading;

namespace Panama.Core.Interfaces
{
    public interface IEvent
    {
        IList<IModel> Data { get; }
    }
}
