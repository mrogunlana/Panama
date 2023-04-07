﻿using Microsoft.Extensions.DependencyInjection;
using Panama.Extensions;
using Panama.Interfaces;
using Panama.Invokers;
using Panama.Resolvers;
using System.Reflection;

namespace Panama
{
    public static class Registrar
    {
        public static void AddPanamaBase(this IServiceCollection services)
        {
            services.AddSingleton<IInvoke<IValidate>, ActionInvoker<IValidate>>();
            services.AddSingleton<IInvoke<IValidate>, ActionInvoker<IValidate>>();

            services.AddSingleton<IInvoke<IValidate>, ActionInvoker<IValidate>>();
            services.AddSingleton<IInvoke<IQuery>, ActionInvoker<IQuery>>();
            services.AddSingleton<IInvoke<ICommand>, ActionInvoker<ICommand>>();
            services.AddSingleton<IInvoke<IRollback>, ActionInvoker<IRollback>>();
            services.AddTransient<IHandler, Handler>();
            services.AddTransient<IInvoke<IHandler>, DefaultInvoker>();
            services.AddTransient<IInvoke<IHandler>, ScopedInvoker>();
            services.AddSingleton<HandlerInvokerResolver>(serviceProvider => name =>
              serviceProvider
              .GetServices<IInvoke<IHandler>>()
              .Where(o => o.GetType().Name.Equals(name, StringComparison.InvariantCultureIgnoreCase))
              .First()
            );
        }
        public static void AddPanama(this IServiceCollection services)
        {
            services.AddPanamaBase();

            AddAssembly(services, Assembly.GetEntryAssembly()!);
        }

        public static void AddPanama(this IServiceCollection services, Assembly assembly)
        {
            services.AddPanamaBase();

            AddAssembly(services, assembly);
        }

        public static void AddPanama(this IServiceCollection services, IEnumerable<Assembly> assemblies)
        {
            services.AddPanamaBase();

            var assembliesToScan = assemblies.Distinct();

            foreach (var assembly in assembliesToScan)
            {
                AddAssembly(services, assembly);
            }
        }

        private static void AddAssembly(IServiceCollection services, Assembly assembly)
        {
            services.AddAssemblyType(typeof(ICommand), assembly, false);
            services.AddAssemblyType(typeof(IQuery), assembly, false);
            services.AddAssemblyType(typeof(IValidate), assembly, false);
            services.AddAssemblyType(typeof(IRollback), assembly, false);
        }
    }
}
