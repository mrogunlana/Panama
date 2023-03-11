using Panama.Core.Interfaces;

namespace Panama.Core.CDC.Interfaces
{
    public interface ISubscribe
    {
        Task Event(IContext context);
    }
}
