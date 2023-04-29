using Panama.Canal.RabbitMQ.Models;

namespace Panama.Canal.RabbitMQ
{
    public static class Setup
    {
        public static Panama.Models.Options.PanamaOptions UseRabbitMq(this Panama.Models.Options.PanamaOptions options, Action<RabbitMQOptions> setup)
        {
            options.Register(new Registrars.Default(
                builder: options.Builder,
                setup: (options) => {
                    setup(options);
                }));

            return options;
        }
    }
}