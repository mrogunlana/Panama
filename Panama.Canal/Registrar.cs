using Microsoft.Extensions.DependencyInjection;
using Panama.Canal.Interfaces;
using Panama.Canal.Services;

namespace Panama.Canal
{
    public static class Registrar
    {
        public static void AddPanamaCDC(this IServiceCollection services)
        {
            services.AddHostedService<Bootstrapper>();
            services.AddSingleton<IBootstrap, Bootstrapper>();
            services.AddSingleton<IManage, Manager>();
            //services.AddSingleton<IService, _Default>();
            //services.AddSingleton<IService, Dispatcher>();
        }
    }
}
