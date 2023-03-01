using Panama.Core.CDC.Interfaces;
using Panama.Core.Interfaces;

namespace Panama.Core.CDC.MySQL.Processors
{
    public class ReceivedRetryProcessor : IProcess
    {
        public ReceivedRetryProcessor(IServiceProvider provider)
        {

        }

        public async Task Invoke(IContext context)
        {
        }
    }
}