using Panama.Extensions;
using Panama.Interfaces;

namespace Panama.Models
{
    public class Snapshot<T> : ISnapshot<T>
        where T : IModel
    {
        private readonly T _value;
        public Snapshot(T value)
        {
            _value = value.Copy<T>();
        }

        public T Value => _value;
    }
}