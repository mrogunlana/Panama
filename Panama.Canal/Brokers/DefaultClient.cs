using Microsoft.Extensions.ObjectPool;
using Microsoft.Extensions.Options;
using Panama.Canal.Brokers.Interfaces;
using Panama.Canal.Models;
using Panama.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Panama.Canal.Brokers
{
    public class DefaultClient : IBrokerClient
    {
        private readonly string _queue;
        private readonly string _exchange;
        private readonly object _sync = new();
        private readonly DefaultOptions _options;
        private readonly IPooledObjectPolicy<DefaultConnection> _models;
        private List<IObserver<DefaultEvent>> _observers;
        public Func<TransientMessage, object?, Task>? OnCallback { get; set; }

        public DefaultClient(
              string queue
            , IOptions<CanalOptions> canal
            , IOptions<DefaultOptions> options
            , IPooledObjectPolicy<DefaultConnection> models)
        {
            _queue = queue;
            _models = models;
            _options = options.Value;
            _exchange = $"{options.Value.Exchange}.{canal.Value.Version}";
        }

        public void Commit(object? sender)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public void Listen(TimeSpan timeout, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public void Reject(object? sender)
        {
            throw new NotImplementedException();
        }

        public void Subscribe(IEnumerable<string> topics)
        {
            throw new NotImplementedException();
        }
    }
}
