using Panama.Extensions;
using Panama.Interfaces;

namespace Panama.Models
{
    public class Filter<T> : IFilter<T>
        where T : IModel
    {
        private readonly T _value;
        public Filter(T value)
        {
            _value = value.Copy<T>();
        }

        public T Value => _value;
    }
}