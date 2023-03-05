﻿using Panama.Core.Models;

namespace Panama.Core.CDC
{
    public class MessageContext : Context
    {
        public MessageContext(
              _Message data
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