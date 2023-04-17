using Panama.Canal.MySQL.Models;
using Panama.Extensions;

namespace Panama.Canal.MySQL
{
    public static class Setup
    {
        public static Panama.Models.Options.PanamaOptions UseMysql(this Panama.Models.Options.PanamaOptions options, Action<MySqlOptions>? setup = null)
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