using Panama.Interfaces;
using Panama.Models;

namespace Panama.Canal.Models.Filters
{
    public class Queued<T> : Filter<T>
        where T : IModel
    {
        public Queued(T value)
            : base(value) { }
    }
}