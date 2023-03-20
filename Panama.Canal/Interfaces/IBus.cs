using Panama.Canal.Models;
using Panama.Interfaces;
using System.Collections.Generic;

namespace Panama.Canal.Interfaces
{
    public interface IBus
    {
        BusContext Context { get; }

        Task Publish(CancellationToken? token = null);
    }
}
