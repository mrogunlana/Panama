using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Panama.Extensions;
using System.Reflection;

namespace Panama
{
    public static class Setup
    {
        public static IServiceCollection AddPanama(this IServiceCollection services, IConfiguration? configuration = null, IEnumerable<Assembly>? assemblies = null, Action<Models.Options.PanamaOptions>? setup = null)
        {
            if (setup == null)
                setup = (options) => { };

            var options = new Models.Options.PanamaOptions();
            
            options.SetBuilder(new Models.Builder(options, configuration, assemblies));
            options.Register(new Registrars.Default(builder: options.Builder));
            options.Register(new Registrars.Security(builder: options.Builder));

            setup(options);

            foreach (var registrar in options.Builder.Registrars)
            {
                if (services.Exist(registrar.Marker))
                    continue;

                registrar.AddServices(services);
                registrar.AddAssemblies(services);
                registrar.AddConfigurations(services);
            }

            services.Configure(setup);

            return services;
        }
    }
}