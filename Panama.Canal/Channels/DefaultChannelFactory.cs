using Microsoft.Extensions.DependencyInjection;
using Panama.Canal.Interfaces;

namespace Panama.Canal.Channels
{
    public class DefaultChannelFactory : IDefaultChannelFactory
    {
        private readonly IServiceProvider _provider;

        public DefaultChannelFactory(
            IServiceProvider provider)
        {
            _provider = provider;
        }

        public T CreateChannel<T>(CancellationToken token = default) where T : IChannel
        {
            token.ThrowIfCancellationRequested();

            var channel = _provider.GetRequiredService<T>();

            return channel;
        }
    }
}
