using Microsoft.Extensions.DependencyInjection;
using Panama.Interfaces;
using Panama.Models;
using Panama.Security;
using Panama.Security.Interfaces;
using Panama.Security.Resolvers;

namespace Panama.Registrars
{
    public class Security : IRegistrar
    {
        private readonly Builder _builder;
        private readonly Action<Models.Options.SecurityOptions> _setup;

        public Type Marker => typeof(Models.Markers.Security);

        public Security(
            Builder builder,
            Action<Models.Options.SecurityOptions>? setup = null)
        {
            _builder = builder;
            _setup = setup ?? ((options) => { });
        }

        public void AddServices(IServiceCollection services)
        {
            services.AddSingleton<IStringEncryptor, Base64Encryptor>();
            services.AddSingleton<IStringEncryptor, SHA256CryptoServiceEncryptor>();
            services.AddSingleton<IStringEncryptor, AESEncryptor>();
            services.AddSingleton<StringEncryptorResolver>(serviceProvider => key =>
              serviceProvider.GetServices<IStringEncryptor>().First(o => o.Key.Equals(key))
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
        }
    }
}