using System;
using System.Collections.Generic;
using System.Threading;

namespace Panama.Core.Interfaces
{
    public interface IContext
    {
        Guid HandlerId { get; }
        Guid CorrelationId { get; }
        IList<IModel> Data { get; }
        CancellationToken Token { get; }
    }
}
