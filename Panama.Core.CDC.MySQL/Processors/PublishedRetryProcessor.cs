using Panama.Core.CDC.Interfaces;
using Panama.Core.Interfaces;

namespace Panama.Core.CDC.MySQL.Processors
{
    public class PublishedRetryProcessor : IProcess
    {
        public PublishedRetryProcessor(IServiceProvider provider)
        {
            
        }

        public async Task Invoke(IContext context)
        {
        }
    }
}