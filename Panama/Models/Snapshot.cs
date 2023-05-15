using Panama.Interfaces;

namespace Panama.Models
{
    public class Snapshot<T> : Filter<T>
        where T : IModel
    {
        public Snapshot(T value)
            : base(value) { }
    }
}