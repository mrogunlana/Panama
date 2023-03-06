using Panama.Core.CDC.Interfaces;
using Panama.Core.Interfaces;

namespace Panama.Core.CDC.Processors
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