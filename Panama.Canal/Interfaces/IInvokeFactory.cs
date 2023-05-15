using Panama.Interfaces;

namespace Panama.Canal.Interfaces
{
    public interface IInvokeFactory
    {
        IInvoke GetInvoker();
    }
}