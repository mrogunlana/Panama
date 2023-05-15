using Panama.Interfaces;

namespace Panama.Canal.Interfaces
{
    public interface ISubscribe
    {
        Task Event(IContext context);
    }
}
