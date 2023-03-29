using Panama.Interfaces;
using Panama.Models;

namespace Panama.Canal.Models
{
    public class Publish<T> : Filter<T>
        where T : IModel
    {
        public Publish(T value)
            : base(value) { }
    }
}