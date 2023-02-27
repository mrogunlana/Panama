using Panama.Core.Interfaces;

namespace Panama.Core.Messaging.Interfaces
{
    public interface ISubscribe<T> where T : IBroker
    {
        string Topic { get; }
        Task Execute(IContext context);
    }
}
