using Panama.Canal.Models;
using Panama.Interfaces;

namespace Panama.Canal.Interfaces
{
    public interface IBus
    {
        EventContext Context { get; }

        Task<IResult> Post(CancellationToken? token = null);
        Task<IResult> Post(InternalMessage message, CancellationToken? token = null);
    }
}
