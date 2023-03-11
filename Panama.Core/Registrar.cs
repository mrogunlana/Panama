using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Panama.Core.Interfaces;
using Panama.Core.Invokers;
using Panama.Core.Configuration;
using Panama.Core.Models;
using Microsoft.Extensions.Logging;

namespace Panama.Core.Service
{
    public static class Registrar
    {
        public static void AddPanama(this IServiceCollection services)
        {
            services.AddSingleton<IInvokeAction, InvokeActions>();
            services.AddSingleton<IHandler, Handler>();
            services.AddSingleton<IInvokeHandler<IHandler>, InvokeHandler>();
            AddAssembly(services, Assembly.GetEntryAssembly());
        }
        
        public static void AddPanama(this IServiceCollection services, Assembly assembly)
        {
            services.AddSingleton<IInvokeAction, InvokeActions>(); 
            services.AddSingleton<IHandler, Handler>();
            services.AddSingleton<IInvokeHandler<IHandler>, InvokeHandler>();
            AddAssembly(services, assembly);            
        }

        public static void AddPanama(this IServiceCollection services, IEnumerable<Assembly> assemblies)
        {
            var assembliesToScan = assemblies.Distinct();
            services.AddSingleton<IInvokeAction, InvokeActions>(); 
            services.AddSingleton<IHandler, Handler>();
            services.AddSingleton<IInvokeHandler<IHandler>, InvokeHandler>();
            foreach (var assembly in assembliesToScan)
            {
                AddAssembly(services, assembly);
            }
        }

        private static void AddAssembly(IServiceCollection services, Assembly assembly)
        {
            AddAssemblyType(services, typeof(ICommand), assembly);
            AddAssemblyType(services, typeof(IQuery), assembly);
            AddAssemblyType(services, typeof(IValidate), assembly);
            AddAssemblyType(services, typeof(IRollback), assembly);
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
