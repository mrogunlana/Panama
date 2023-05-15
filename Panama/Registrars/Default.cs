using Microsoft.Extensions.DependencyInjection;
using Panama.Extensions;
using Panama.Interfaces;
using Panama.Invokers;
using Panama.Models;
using Panama.Resolvers;

namespace Panama.Registrars
{
    public class Default : IRegistrar
    {
        private readonly Builder _builder;
        private readonly Action<Models.Options.PanamaOptions> _setup;

        public Type Marker => typeof(Models.Markers.Default);

        public Default(
            Builder builder,
            Action<Models.Options.PanamaOptions>? setup = null)
        {
            _builder = builder;
            _setup = setup ?? ((options) => { });
        }

        public void AddServices(IServiceCollection services)
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

        public void AddConfigurations(IServiceCollection services)
        {
            if (_builder.Configuration == null)
                return;

            services.Configure(_setup);
        }

        public void AddAssemblies(IServiceCollection services)
        {
            if (_builder.Assemblies == null)
                return;

            services.AddAssemblyTypes<ICommand>(_builder.Assemblies.Distinct(), false);
            services.AddAssemblyTypes<IQuery>(_builder.Assemblies.Distinct(), false);
            services.AddAssemblyTypes<IValidate>(_builder.Assemblies.Distinct(), false);
            services.AddAssemblyTypes<IRollback>(_builder.Assemblies.Distinct(), false);
        }
    }
}