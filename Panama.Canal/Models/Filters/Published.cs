using Panama.Interfaces;
using Panama.Models;

namespace Panama.Canal.Models.Filters
{
    public class Published<T> : Filter<T>
        where T : IModel
    {
        public Published(T value)
            : base(value) { }
    }
}