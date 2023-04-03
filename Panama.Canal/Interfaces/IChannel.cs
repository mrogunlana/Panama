using Panama.Canal.Models;
using Panama.Interfaces;
using System.Collections.Concurrent;

namespace Panama.Canal.Interfaces
{
    public interface IChannel<C, T> : IChannel
    {
        T? Current { get; }
        void Open(C channel, CancellationToken token = default);
    }

    public interface IChannel : IDisposable
    {
        ConcurrentQueue<InternalMessage> Queue { get; }
        EventContext Context { get; }
        IInvoke Invoker { get; set; }

        void Rollback(CancellationToken token = default);
        Task Commit(CancellationToken token = default);
        Task Flush(CancellationToken token = default);
        Task Post(
              string name
            , string? ack = null
            , string? nack = null
            , string? group = null
            , DateTime? delay = null
            , string? instance = null
            , string? correlationId = null
            , CancellationToken token = default
            , IDictionary<string, string?>? headers = null
            , params IModel[]? data);

        Task Post<T>(
              string name
            , string? ack = null
            , string? nack = null
            , string? group = null
            , DateTime? delay = null
            , string? instance = null
            , string? correlationId = null
            , CancellationToken token = default
            , IDictionary<string, string?>? headers = null
            , params IModel[]? data)
            where T : ITarget;

        Task Post<T, I>(
              string name
            , string? ack = null
            , string? nack = null
            , string? group = null
            , DateTime? delay = null
            , string? instance = null
            , string? correlationId = null
            , CancellationToken token = default
            , IDictionary<string, string?>? headers = null
            , params IModel[]? data)
            where T : ITarget
            where I : IInvoke;
    }
}