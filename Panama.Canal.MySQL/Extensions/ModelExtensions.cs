using Panama.Interfaces;
using System.Reflection;

namespace Panama.Canal.MySQL.Extensions
{
    internal static class ModelExtensions
    {
        internal static void SetValue<T>(this IModel model, string name, object? value)
        {
            var property = typeof(T).GetProperty(name, BindingFlags.Public | BindingFlags.Instance);

            if (property == null)
                return;
            if (!property.CanWrite)
                return;

            var result = Convert.ChangeType(value, property.PropertyType);

            property.SetValue(model, result, null);
        }

        internal static T Resolve<T>(this IModel model)
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
