using Panama.Core.CDC.Interfaces;
using Panama.Core.Interfaces;

namespace Panama.Core.CDC.Processors
{
    public class DeleteExpiredProcess : IProcess
    {
        public DeleteExpiredProcess(IServiceProvider provider)
        {
            
        }

        public async Task Invoke(IContext context)
        {
        }
    }
}