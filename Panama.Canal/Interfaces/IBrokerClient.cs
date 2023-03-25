using Panama.Canal.Models;

namespace Panama.Canal.Interfaces
{
    public interface IBrokerClient
    {
        void Subscribe(IEnumerable<string> topics);
        void Listening(TimeSpan timeout, CancellationToken cancellationToken);
        void Commit(object? sender);
        void Reject(object? sender);

        public Func<InternalMessage, object?, Task>? Callback { get; set; }
    }
}