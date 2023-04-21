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

        public static CanalOptions UseDefaultBroker(this CanalOptions options, Action<BrokerOptions>? setup = null)
        {
            options.Register(new Registrars.Broker(
                builder: options.Builder,
                setup: (options) => {
                    if (setup == null) return;
                    setup(options);
                }));

            return options;
        }

        public static CanalOptions UseDefaultStore(this CanalOptions options, Action<StoreOptions>? setup = null)
        {
            options.Register(new Registrars.Store(
                builder: options.Builder,
                setup: (options) => {
                    if (setup == null) return;
                    setup(options);
                }));

            return options;
        }

        public static CanalOptions UseDefaultScheduler(this CanalOptions options, Action<SchedulerOptions>? setup = null)
        {
            options.Register(new Registrars.Scheduler(
                builder: options.Builder,
                setup: (options) => {
                    if (setup == null) return;
                    setup(options);
                }));

            return options;
        }
    }
}