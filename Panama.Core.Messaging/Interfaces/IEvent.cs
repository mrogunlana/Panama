using System.Collections.Generic;
using System.Threading;

namespace Panama.Core.Messaging.Interfaces
{
    public interface IEvent
    {
        IList<IModel> Data { get; }
    }
}
