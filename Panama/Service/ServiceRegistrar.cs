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
        public static void RegisterPanama(this IServiceCollection services, IEnumerable<Assembly> assemblies)
        {
            var assembliesToScan = assemblies.Distinct();
            RegisterInterface(services, typeof(ICommand), assembliesToScan);
            RegisterInterface(services, typeof(ICommandAsync), assembliesToScan);
            RegisterInterface(services, typeof(IValidation), assembliesToScan);
            RegisterInterface(services, typeof(IRollback), assembliesToScan);
        }


        private static void RegisterInterface(IServiceCollection services, Type type, IEnumerable<Assembly> assembliesToScan)
        {
            foreach (var assembly in assembliesToScan)
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
}
