using Microsoft.Extensions.DependencyInjection;
using Panama.Interfaces;
using Panama.Invokers;
using Panama.Resolvers;
using System.Reflection;

namespace Panama.Service
{
    public static class Registrar
    {
        public static void AddPanama(this IServiceCollection services)
        {
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

            

            AddAssembly(services, Assembly.GetEntryAssembly()!);
        }
        
        public static void AddPanama(this IServiceCollection services, Assembly assembly)
        {
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
            AddAssembly(services, assembly);            
        }

        public static void AddPanama(this IServiceCollection services, IEnumerable<Assembly> assemblies)
        {
            var assembliesToScan = assemblies.Distinct();
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
