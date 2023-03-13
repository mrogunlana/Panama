using Panama.Interfaces;

namespace Panama.Canal.Interfaces
{
    public interface IInitialize
    {
        IModel Settings { get; }
        Task Invoke(CancellationToken token);
    }
}
