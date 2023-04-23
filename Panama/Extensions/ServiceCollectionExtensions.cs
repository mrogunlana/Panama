using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Panama.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddAssemblyType(this IServiceCollection services, Type type, Assembly assembly, bool singleton = true)
        {
            var types = from t in assembly.GetTypes()
                               where type.IsAssignableFrom(t) && type.Name != t.Name
                               select t;
            foreach (var value in types)
            {
                if (value.IsInterface)
                    continue;
                if (singleton)
                    services.AddSingleton(value);
                else
                    services.AddTransient(value);
            }

            return services;
        }

        public static IServiceCollection AddAssemblyTypeWithInterface<T>(this IServiceCollection services, Assembly assembly, bool singleton = true)
        {
            var type = typeof(T);
            var types = from t in assembly.GetTypes()
                        where type.IsAssignableFrom(t) && type.Name != t.Name
                        select t;
            foreach (var value in types)
            {
                if (value.IsInterface)
                    continue;
                if (singleton)
                {
                    services.AddSingleton(value);
                    services.AddSingleton(typeof(T), p => p.GetRequiredService(value));
                }
                else
                {
                    services.AddTransient(value);
                    services.AddTransient(typeof(T), p => p.GetRequiredService(value));
                }
            }

            return services;
        }

        public static IServiceCollection AddAssemblyTypesWithInterface<T>(this IServiceCollection services, IEnumerable<Assembly> assemblies, bool singleton = true)
        {
            var scan = assemblies.Distinct();

            foreach (var assembly in scan)
                services.AddAssemblyTypeWithInterface<T>(assembly, singleton);

            return services;
        }

        public static IServiceCollection AddAssemblyTypes<T>(this IServiceCollection services, IEnumerable<Assembly> assemblies, bool singleton = true)
        {
            var scan = assemblies.Distinct();

            foreach (var assembly in scan)
                services.AddAssemblyType(typeof(T), assembly, singleton);

            return services;
        }

        public static IServiceCollection AddAssemblyTypeByInterface<T>(this IServiceCollection services, Assembly assembly, bool singleton = true)
        {
            var type = typeof(T);
            var types = from t in assembly.GetTypes()
                        where type.IsAssignableFrom(t) && type.Name != t.Name
                        select t;
            foreach (var value in types)
            {
                if (value.IsInterface)
                    continue;
                if (singleton)
                    services.AddSingleton(typeof(T), value);
                else
                    services.AddTransient(typeof(T), value);
            }

            return services;
        }

        public static IServiceCollection AddAssemblyTypesByInterface<T>(this IServiceCollection services, IEnumerable<Assembly> assemblies, bool singleton = true)
        {
            var scan = assemblies.Distinct();

            foreach (var assembly in scan)
                services.AddAssemblyTypeByInterface<T>(assembly, singleton);

            return services;
        }

        public static IServiceCollection Remove<T>(this IServiceCollection services)
        {
            services.Remove(typeof(T));

            return services;
        }

        public static IServiceCollection Remove(this IServiceCollection services, Type type)
        {
            if (services.IsReadOnly)
                throw new Exception($"{nameof(services)} is read only");

            var descriptors = services.Where(descriptor => descriptor.ServiceType == type);
            if (descriptors == null)
                return services;

            foreach (var descriptor in descriptors)
                services.Remove(descriptor);

            return services;
        }

        public static bool Exist<T>(this IServiceCollection services)
        {
            return services.Exist(typeof(T));
        }

        public static bool Exist(this IServiceCollection services, Type type)
        {
            var descriptors = services.Where(descriptor => descriptor.ServiceType == type);
            if (descriptors == null)
                return false;
            if (descriptors.Count() == 0)
                return false;

            return true;
        }
    }
}
