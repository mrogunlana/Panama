using Panama.Canal.Models;
using Panama.Interfaces;
using System.Collections.Concurrent;

namespace Panama.Canal.Interfaces
{
    public interface IInvokeFactory
    {
        IInvoke GetInvoker();
    }
}