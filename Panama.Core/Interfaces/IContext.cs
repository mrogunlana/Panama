using System;
using System.Collections.Generic;
using System.Threading;

namespace Panama.Core.Interfaces
{
    public interface IContext
    {
        string Id { get; }
        string CorrelationId { get; }
        IList<IModel> Data { get; }
        CancellationToken Token { get; set; }
        IServiceProvider Provider { get; }
    }
}
