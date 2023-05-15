using Panama.Canal.Models.Options;
using Panama.Canal.MySQL.Models;

namespace Panama.Canal.MySQL
{
    public static class Setup
    {
        public static CanalOptions UseMySqlStore(this CanalOptions options, Action<MySqlOptions>? setup = null)
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