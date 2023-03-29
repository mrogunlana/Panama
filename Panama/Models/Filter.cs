using Panama.Extensions;
using Panama.Interfaces;

namespace Panama.Models
{
    public class Filter<T> : IFilter<T>
        where T : IModel
    {
        private readonly T _value;
        private readonly string _id;
        public Filter(T value)
        {
            _value = value.Copy<T>();
            _id = string.Empty;
        }

        public T Value => _value;
        public string? Id => _id;
    }
}