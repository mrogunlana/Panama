using Panama.Canal.Models.Options;
using Panama.Canal.RabbitMQ.Models;

namespace Panama.Canal.RabbitMQ
{
    public static class Setup
    {
        public static CanalOptions UseRabbitMq(this CanalOptions options, Action<RabbitMQOptions>? setup = null)
        {
            options.Register(new Registrars.Default(
                builder: options.Builder,
                setup: (options) => {
                    if (setup == null) return;
                    setup(options);
                }));

            return options;
        }
    }
}