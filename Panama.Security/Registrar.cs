using Microsoft.Extensions.DependencyInjection;
using Panama.Security.Interfaces;
using Panama.Security.Resolvers;
using System.Linq;

namespace Panama.Security
{
    public static class Registrar
    {
        public static void AddPanamaCoreSecurity(this IServiceCollection services)
        {
            services.AddSingleton<IStringEncryptor, Base64Encryptor>();
            services.AddSingleton<IStringEncryptor, SHA256CryptoServiceEncryptor>();
            services.AddSingleton<IStringEncryptor, AESEncryptor>();
            services.AddSingleton<StringEncryptorResolver>(serviceProvider => key =>            
              serviceProvider.GetServices<IStringEncryptor>().First(o => o.Key.Equals(key))
            );
        }
    }
}
