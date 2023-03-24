using Microsoft.Extensions.DependencyInjection;
using Panama.Interfaces;
using System.Reflection;

namespace Panama.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static void AddAssemblyType(this IServiceCollection services, Type type, Assembly assembly, bool singleton = true)
        {
            var commandTypes = from t in assembly.GetTypes()
                               where type.IsAssignableFrom(t) && type.Name != t.Name
                               select t;
            foreach (var commandType in commandTypes)
            {
                if (singleton)
                    services.AddSingleton(commandType);
                else
                    services.AddTransient(commandType);
            }
        }
    }
}
