using Panama.Canal.Models;

namespace Panama.Canal.Interfaces
{
    public interface IBrokerClient : IDisposable
    {
        void Subscribe(IEnumerable<string> topics);
        void Listening(TimeSpan timeout, CancellationToken cancellationToken);
        void Commit(object? sender);
        void Reject(object? sender);

        public Func<TransientMessage, object?, Task>? OnCallback { get; set; }
    }
}