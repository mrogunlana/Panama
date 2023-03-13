using Panama.Interfaces;

namespace Panama.Canal.Interfaces
{
    public interface IBus
    {
        IServiceProvider ServiceProvider { get; }

        AsyncLocal<ITransaction> Transaction { get; }

        Task PublishAsync<D>(string name
            , D? data
            , string? ack = null
            , string? nack = null
            , CancellationToken cancellationToken = default
            , string correlationId = "")
            where D : IModel;

        Task PublishAsync<D>(string name
            , D? data
            , IDictionary<string, string?> headers
            , string? ack = null
            , string? nack = null
            , CancellationToken cancellationToken = default
            , string correlationId = "")
            where D : IModel;

        Task PublishAsync<D, T>(string name
            , D? data
            , IDictionary<string, string?> headers
            , string? ack = null
            , string? nack = null
            , CancellationToken cancellationToken = default
            , string correlationId = "")
            where D : IModel
            where T : ITarget;

        Task PublishDelayAsync<D>(TimeSpan delay
            , string name
            , D? data
            , string? ack = null
            , string? nack = null
            , CancellationToken cancellationToken = default
            , string correlationId = "")
            where D : IModel;

        Task PublishDelayAsync<D>(TimeSpan delay
            , string name
            , D? data
            , IDictionary<string, string?> headers
            , string? ack = null
            , string? nack = null
            , CancellationToken cancellationToken = default
            , string correlationId = "")
            where D : IModel;

        Task PublishDelayAsync<D, T>(TimeSpan delay
            , string name
            , D? data
            , IDictionary<string, string?> headers
            , string? ack = null
            , string? nack = null
            , CancellationToken cancellationToken = default
            , string correlationId = "")
            where D : IModel
            where T : ITarget;
    }
}
