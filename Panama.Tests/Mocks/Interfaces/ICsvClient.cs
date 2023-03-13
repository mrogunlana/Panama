using System.Collections.Generic;
using System.Threading.Tasks;

namespace Panama.Tests
{
    public interface ICsvClient
    {
        Task<IEnumerable<T>> Get<T>(string key);

        Task Save<T>(string key, List<T> models);
    }
}
