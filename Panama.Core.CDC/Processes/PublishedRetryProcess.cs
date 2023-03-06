using Panama.Core.CDC.Interfaces;
using Panama.Core.Interfaces;

namespace Panama.Core.CDC.Processors
{
    public class PublishedRetryProcess : IProcess
    {
        public PublishedRetryProcess(IServiceProvider provider)
        {
            
        }

        public async Task Invoke(IContext context)
        {
        }
    }
}