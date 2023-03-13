using Panama.Core.Extensions;
using Panama.Core.Interfaces;

namespace Panama.Core.Models
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