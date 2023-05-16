using Panama.Interfaces;
using System.Reflection;

namespace Panama.Canal.MySQL.Extensions
{
    public static class ModelExtensions
    {
        public static void SetValue<T>(this IModel model, string name, object? value)
        {
            var property = typeof(T).GetProperty(name, BindingFlags.Public | BindingFlags.Instance);

            if (property == null)
                return;
            if (!property.CanWrite)
                return;

            var type = property.PropertyType;
            
            type = Nullable.GetUnderlyingType(type) ?? type;

            if (value == null)
                return;
            if (value is DBNull) 
                return;

            var result = Convert.ChangeType(value, type);

            property.SetValue(model, result, null);
        }

        public static T Resolve<T>(this IModel model)
            where T : IModel
        {
            if (model == null)
                throw new ArgumentException($"Model must have a value to resolve type: {typeof(T)}.");

            if (model is not T)
                throw new ArgumentException($"Model must be of type: {typeof(T)} to resolve.");

            return (T)model;
        }
    }
}
