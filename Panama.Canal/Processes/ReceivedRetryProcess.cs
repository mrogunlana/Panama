using Panama.Canal.Interfaces;
using Panama.Interfaces;

namespace Panama.Canal.Processors
{
    public class ReceivedRetryProcess : IProcess
    {
        public ReceivedRetryProcess(IServiceProvider provider)
        {

        }

        public async Task Invoke(IContext context)
        {
        }
    }
}