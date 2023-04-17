using Panama.Interfaces;

namespace Panama.Extensions
{
    public static class OptionsExtensions
    {
        public static void Register(this Models.Options.PanamaOptions options, IRegistrar registrar)
        {
            if (options.Builder == null)
                throw new ArgumentNullException(nameof(options));

            options.Builder.Register(registrar);
        }
    }
}
