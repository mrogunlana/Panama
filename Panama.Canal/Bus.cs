using Microsoft.Extensions.Logging;
using Panama.Canal.Extensions;
using Panama.Canal.Interfaces;
using Panama.Canal.Models;
using Panama.Interfaces;

namespace Panama.Canal
{
    public class Bus : IBus
    {
        private readonly ILogger<Bus> _log;
        private readonly IDispatcher _dispatcher;
        public IServiceProvider ServiceProvider { get; }
        public AsyncLocal<ITransaction> Transaction { get; }

        public Bus(
              IServiceProvider provider
            , IDispatcher dispatcher
            , ILogger<Bus> log)
        {
            _log = log;
            _dispatcher = dispatcher;

            ServiceProvider = provider;
            Transaction = new AsyncLocal<ITransaction>();
        }

        public async Task PublishAsync<D>(string name
            , D? data
            , string? ack = null
            , string? nack = null
            , CancellationToken cancellationToken = default
            , string correlationId = "") 
            where D : IModel
        {
            var message = new Message()
                .AddCorrelationId(correlationId)
                .AddData(data)
                .AddAck(ack)
                .AddNack(nack)
                .ToInternal(ServiceProvider);

            await _dispatcher.Publish(message).ConfigureAwait(false);
        }

        public async Task PublishAsync<D>(string name
            , D? data
            , IDictionary<string, string?> headers
            , string? ack = null
            , string? nack = null
            , CancellationToken cancellationToken = default
            , string correlationId = "") 
            where D : IModel
        {
            var message = new Message()
                .AddCorrelationId(correlationId)
                .AddHeaders(headers)
                .AddData(data)
                .AddAck(ack)
                .AddNack(nack)
                .ToInternal(ServiceProvider);

            await _dispatcher.Publish(message).ConfigureAwait(false);
        }

        public async Task PublishAsync<D, T>(string name
            , D? data
            , IDictionary<string, string?> headers
            , string? ack = null
            , string? nack = null
            , CancellationToken cancellationToken = default
            , string correlationId = "")
            where D : IModel
            where T : ITarget
        {
            var message = new Message()
                .AddCorrelationId(correlationId)
                .AddHeaders(headers)
                .AddMessageGroup(nameof(T))
                .AddData(data)
                .AddAck(ack)
                .AddNack(nack)
                .ToInternal(ServiceProvider);

            await _dispatcher.Publish(message).ConfigureAwait(false);
        }

        public async Task PublishDelayAsync<D>(TimeSpan delay
            , string name
            , D? data
            , string? ack = null
            , string? nack = null
            , CancellationToken cancellationToken = default
            , string correlationId = "") 
            where D : IModel
        {
            var message = new Message()
                .AddCorrelationId(correlationId)
                .AddDelayTime(delay)
                .AddData(data)
                .AddAck(ack)
                .AddNack(nack)
                .ToInternal(ServiceProvider);

            await _dispatcher.Publish(message).ConfigureAwait(false);
        }

        public async Task PublishDelayAsync<D>(TimeSpan delay
            , string name
            , D? data
            , IDictionary<string, string?> headers
            , string? ack = null
            , string? nack = null
            , CancellationToken cancellationToken = default
            , string correlationId = "") 
            where D : IModel
        {
            var message = new Message()
                .AddCorrelationId(correlationId)
                .AddHeaders(headers)
                .AddDelayTime(delay)
                .AddData(data)
                .AddAck(ack)
                .AddNack(nack)
                .ToInternal(ServiceProvider);

            await _dispatcher.Publish(message).ConfigureAwait(false);
        }

        public async Task PublishDelayAsync<D, T>(TimeSpan delay
            , string name
            , D? data
            , IDictionary<string, string?> headers
            , string? ack = null
            , string? nack = null
            , CancellationToken cancellationToken = default
            , string correlationId = "")
            where D : IModel
            where T : ITarget
        {
            var message = new Message()
                .AddCorrelationId(correlationId)
                .AddHeaders(headers)
                .AddMessageGroup(nameof(T))
                .AddDelayTime(delay)
                .AddData(data)
                .AddAck(ack)
                .AddNack(nack)
                .ToInternal(ServiceProvider);

            await _dispatcher.Publish(message).ConfigureAwait(false);
        }
    }
}