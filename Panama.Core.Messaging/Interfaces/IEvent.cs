using Panama.Core.Interfaces;

namespace Panama.Core.Messaging.Interfaces
{
    public interface IEvent
    {
        IList<IModel> Data { get; }
    }
}
