﻿using Microsoft.Extensions.DependencyInjection;
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
                if (singleton)
                    services.AddSingleton(value);
                else
                    services.AddTransient(value);
            }

            return services;
        }

        public static IServiceCollection AddAssemblyTypes<T>(this IServiceCollection services, IEnumerable<Assembly> assemblies, bool singleton = true)
        {
            var scan = assemblies.Distinct();

            foreach (var assembly in scan)
                services.AddAssemblyType(typeof(T), assembly, singleton);

            return services;
        }

        public static IServiceCollection Remove<T>(this IServiceCollection services)
        {
            if (services.IsReadOnly)
                throw new Exception($"{nameof(services)} is read only");

            var descriptors = services.Where(descriptor => descriptor.ServiceType == typeof(T));
            if (descriptors == null)
                return services;

            foreach (var descriptor in descriptors)
                services.Remove(descriptor);

            return services;
        }

        public static bool Exist<T>(this IServiceCollection services)
        {
            var descriptors = services.Where(descriptor => descriptor.ServiceType == typeof(T));
            if (descriptors == null)
                return false;
            if (descriptors.Count() == 0)
                return false;

            return true;
        }
    }
}
