using Panama.Interfaces;

namespace Panama.Canal.Interfaces
{
    public interface IManage
    {
        Task Invoke(IContext context);
    }
}
