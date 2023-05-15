using Microsoft.Extensions.DependencyInjection;
using Panama.Canal.Interfaces;

namespace Panama.Canal.MySQL.Channels
{
    public class MySqlChannelFactory : IGenericChannelFactory
    {
        private readonly IServiceProvider _provider;

        public MySqlChannelFactory(
            IServiceProvider provider)
        {
            _provider = provider;
        }

        public IChannel<C, T> CreateChannel<C, T>(C client, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();

            var channel = _provider.GetRequiredService<IChannel<C,T>>();

            channel.Open(client, token);

            return channel;
        }
    }
}
