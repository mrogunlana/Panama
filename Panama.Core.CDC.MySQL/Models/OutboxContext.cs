using Panama.Core.Interfaces;
using Panama.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Panama.Core.CDC.MySQL
{
    public class OutboxContext : Context
    {
        public OutboxContext(
              Outbox data
            , IServiceProvider? provider = null
            , CancellationToken? token = null)
            : base(provider, token)
        {
            Data.Add(data);

            Id = data.Id;
            CorrelationId = data.CorrelationId;
        }
    }
}
