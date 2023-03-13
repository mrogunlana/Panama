using Panama.Canal.Interfaces;
using Panama.Interfaces;

namespace Panama.Canal.Processors
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