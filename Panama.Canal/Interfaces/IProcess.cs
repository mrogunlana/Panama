using Panama.Interfaces;

namespace Panama.Canal.Interfaces
{
    public interface IProcess
    {
        Task Invoke(IContext context);
    }
}
