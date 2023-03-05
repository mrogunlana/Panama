using Panama.Core.Interfaces;

namespace Panama.Core.CDC.Interfaces
{
    public interface IInitialize
    {
        IModel Settings { get; }
        Task Invoke(CancellationToken token);
    }
}
