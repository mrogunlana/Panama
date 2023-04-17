using Panama.Canal.Brokers;
using Panama.Canal.Models.Options;
using Panama.Extensions;

namespace Panama.Canal
{
    public static class Setup
    {
        public static Panama.Models.Options.PanamaOptions UseCanal(this Panama.Models.Options.PanamaOptions options, Action<CanalOptions>? setup = null)
        {
            options.Register(new Registrars.Default(
                builder: options.Builder,
                setup: (options) => {
                    if (setup == null) return;
                    setup(options);
                }));

            return options;
        }

        public static Panama.Models.Options.PanamaOptions UseDefaultBroker(this Panama.Models.Options.PanamaOptions options, Action<DefaultOptions>? setup = null)
        {
            options.Register(new Registrars.Broker(
                builder: options.Builder,
                setup: (options) => {
                    if (setup == null) return;
                    setup(options);
                }));

            return options;
        }

        public static Panama.Models.Options.PanamaOptions UseDefaultStore(this Panama.Models.Options.PanamaOptions options, Action<StoreOptions>? setup = null)
        {
            options.Register(new Registrars.Store(
                builder: options.Builder,
                setup: (options) => {
                    if (setup == null) return;
                    setup(options);
                }));

            return options;
        }
    }
}