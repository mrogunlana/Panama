using Microsoft.Extensions.DependencyInjection;
using Panama.Core.CDC.Interfaces;
using Panama.Core.CDC.Services;

namespace Panama.Core.CDC
{
    public static class Registrar
    {
        public static void AddPanamaCDC(this IServiceCollection services)
        {
            services.AddHostedService<Bootstrapper>();
            services.AddSingleton<IBootstrap, Bootstrapper>();
            services.AddSingleton<IManage, Manager>();
            services.AddSingleton<IService, _Default>();
            services.AddSingleton<IService, Dispatcher>();
        }
    }
}
