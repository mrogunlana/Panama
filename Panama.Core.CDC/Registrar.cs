using Microsoft.Extensions.DependencyInjection;
using Panama.Core.CDC.Interfaces;
using Panama.Core.Interfaces;
using Panama.Core.Invokers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Panama.Core.CDC
{
    public static class Registrar
    {
        public static void AddPanamaCDC(this IServiceCollection services)
        {
            services.AddHostedService<Bootstrapper>();
            services.AddSingleton<IBootstrap, Bootstrapper>();
            services.AddSingleton<IManage, Manager>();
            services.AddSingleton<IServer, Server>();
        }
    }
}
