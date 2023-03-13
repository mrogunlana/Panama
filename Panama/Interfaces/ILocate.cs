using System.Collections.Generic;

namespace Panama.Interfaces
{
    public interface ILocate
    {
        T Resolve<T>();
        IEnumerable<T> ResolveList<T>();
        T Resolve<T>(string name);
    }
}
