using Panama.Core.Interfaces;
using System.Reflection;

namespace Panama.Core.CDC.MySQL.Extensions
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
    }
}
