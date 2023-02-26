using Panama.Core.Interfaces;

namespace Panama.Core.CDC.Interfaces
{
    public interface IManage
    {
        Task Invoke(IContext context);
    }
}
