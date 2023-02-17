using Microsoft.Extensions.DependencyInjection;
using Panama.Core.Commands;
using Panama.Core.Logger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Panama.Core.Service
{
    public static class ServiceRegistrar
    {
        public static void AddPanama(this IServiceCollection services)
        {
            services.AddSingleton<IHandler, Handler>();
            AddAssembly(services, Assembly.GetEntryAssembly());
        }
        
        public static void AddPanama(this IServiceCollection services, Assembly assembly)
        {

            services.AddSingleton<IHandler, Handler>();
            AddAssembly(services, assembly);            
        }

        public static void AddPanama(this IServiceCollection services, IEnumerable<Assembly> assemblies)
        {
            var assembliesToScan = assemblies.Distinct();
            services.AddSingleton<IHandler, Handler>();
            foreach (var assembly in assembliesToScan)
            {
                AddAssembly(services, assembly);
            }
        }

        private static void AddAssembly(IServiceCollection services, Assembly assembly)
        {
            AddAssemblyType(services, typeof(ICommand), assembly);
            AddAssemblyType(services, typeof(ICommandAsync), assembly);
            AddAssemblyType(services, typeof(IValidation), assembly);
            AddAssemblyType(services, typeof(IValidation), assembly);
        }

        private static void AddAssemblyType(IServiceCollection services, Type type, Assembly assembly)
        {
            var commandTypes = from t in assembly.GetTypes()
                               where type.IsAssignableFrom(t) && type.Name != t.Name
                               select t;
            foreach (var commandType in commandTypes)
            {
                services.AddSingleton(commandType);
            }

        }
    }
}
