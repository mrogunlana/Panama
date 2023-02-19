using System.Collections.Generic;
using System.Threading;

namespace Panama.Core.Interfaces
{
    public interface ISubscribe<T> where T : IBroker
    {
        string To { get; }
    }
}
