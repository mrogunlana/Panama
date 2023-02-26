using Panama.Core.Interfaces;

namespace Panama.Core.CDC.Interfaces
{
    public interface IProcess
    {
        Task Invoke(IContext context);
    }
}
