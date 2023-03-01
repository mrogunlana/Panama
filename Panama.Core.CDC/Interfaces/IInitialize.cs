using Panama.Core.Interfaces;

namespace Panama.Core.CDC.Interfaces
{
    public interface IInitialize
    {
        Task Invoke(CancellationToken token);
    }
}
