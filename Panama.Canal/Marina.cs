using Panama.Canal.Extensions;
using Panama.Canal.Interfaces;

namespace Panama.Canal
{
    public class Marina : IMarina
    {
        private readonly IServiceProvider _provider;

        public Marina(IServiceProvider provider)
        {
            _provider = provider;
        }
        public IBus GetBus(CancellationToken? token = null)
        {
            return new Bus(_provider)
                .Token(token);
        }
    }
}