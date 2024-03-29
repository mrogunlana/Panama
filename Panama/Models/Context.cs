﻿using Panama.Interfaces;

namespace Panama.Models
{
    public class Context : IContext
    {
        public Context()
        {
            if (Data == null)
                Data = new List<IModel>();

            if (string.IsNullOrEmpty(Id))
                Id = Guid.NewGuid().ToString();
            if (string.IsNullOrEmpty(CorrelationId))
                CorrelationId = Guid.NewGuid().ToString();
        }
        public Context(
              IServiceProvider provider
            , CancellationToken? token = null)
            : this(token)
        {
            Provider = provider;
        }

        public Context(CancellationToken? token = null)
            : this()
        {
            if (token.HasValue)
                Token = token.Value;
        }
        public Context(
            CancellationToken? token = null
            , string? id = null
            , string? correlationId = null
            , IServiceProvider? provider = null)
            : this()
        {
            if (token.HasValue)
                Token = token.Value;
            if (!string.IsNullOrEmpty(id))
                Id = id;
            if (!string.IsNullOrEmpty(correlationId))
                CorrelationId = correlationId;
            if (provider != null)
                Provider = provider;
        }

        public Context(IEnumerable<IModel> data
            , CancellationToken? token = null
            , string? id = null
            , string? correlationId = null
            , IServiceProvider? provider = null)
            : this(token, id, correlationId, provider)
        {
            Data = data.ToList();
        }

        public Context(
              IModel data
            , CancellationToken? token = null
            , string? id = null
            , string? correlationId = null
            , IServiceProvider? provider = null)
            : this(token, id, correlationId, provider)
        {
            Data.Add(data);
        }

        public Context(
              IModel data
            , CancellationToken? token = null
            , string? id = null
            , string? correlationId = null
            , IServiceProvider? provider = null
            , object? transaction = null)
            : this(data, token, id, correlationId, provider)
        {
            Transaction = transaction;
        }

        public IList<IModel> Data { get; set; }
        public CancellationToken Token { get; set; }
        public string Id { get; set; }
        public string CorrelationId { get; set; }
        public IServiceProvider Provider { get; } = default!;
        public object? Transaction { get; set; } = null;
    }
}
