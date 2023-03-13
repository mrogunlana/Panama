using Microsoft.Extensions.DependencyInjection;
using Panama.Core.Interfaces;
using Panama.Core.Invokers;
using Panama.Core.Resolvers;
using System.Reflection;

namespace Panama.Core.Service
{
    public static class Registrar
    {
        public static void AddPanama(this IServiceCollection services)
        {
            services.AddSingleton<IInvoke<IAction>, ActionInvoker<IAction>>();
            services.AddTransient<IHandler, Handler>();
            services.AddTransient<IInvoke<IHandler>, DefaultInvoker>();
            services.AddTransient<IInvoke<IHandler>, ScopedInvoker>();
            services.AddSingleton<HandlerInvokerResolver>(serviceProvider => name =>
              serviceProvider
              .GetServices<IInvoke<IHandler>>()
              .Where(o => o.GetType().Name.Equals(name, StringComparison.InvariantCultureIgnoreCase))
              .First()
            );
            AddAssembly(services, Assembly.GetEntryAssembly()!);
        }
        
        public static void AddPanama(this IServiceCollection services, Assembly assembly)
        {
            services.AddSingleton<IInvoke<IAction>, ActionInvoker<IAction>>(); 
            services.AddTransient<IHandler, Handler>();
            services.AddTransient<IInvoke<IHandler>, DefaultInvoker>();
            services.AddTransient<IInvoke<IHandler>, ScopedInvoker>();
            services.AddSingleton<HandlerInvokerResolver>(serviceProvider => name =>
              serviceProvider
              .GetServices<IInvoke<IHandler>>()
              .Where(o => o.GetType().Name.Equals(name, StringComparison.InvariantCultureIgnoreCase))
              .First()
            );
            AddAssembly(services, assembly);            
        }

        public static void AddPanama(this IServiceCollection services, IEnumerable<Assembly> assemblies)
        {
            var assembliesToScan = assemblies.Distinct();
            services.AddSingleton<IInvoke<IAction>, ActionInvoker<IAction>>(); 
            services.AddTransient<IHandler, Handler>();
            services.AddTransient<IInvoke<IHandler>, DefaultInvoker>();
            services.AddTransient<IInvoke<IHandler>, ScopedInvoker>();
            services.AddSingleton<HandlerInvokerResolver>(serviceProvider => name =>
              serviceProvider
              .GetServices<IInvoke<IHandler>>()
              .Where(o => o.GetType().Name.Equals(name, StringComparison.InvariantCultureIgnoreCase))
              .First()
            );
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
