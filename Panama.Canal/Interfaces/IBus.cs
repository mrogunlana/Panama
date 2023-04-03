using Panama.Canal.Models;
using Panama.Interfaces;
using System.Collections.Generic;

namespace Panama.Canal.Interfaces
{
    public interface IBus
    {
        EventContext Context { get; }

        Task<IResult> Post(CancellationToken? token = null);
    }
}
