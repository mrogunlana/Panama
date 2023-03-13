using Panama.Canal.Interfaces;
using Panama.Interfaces;

namespace Panama.Canal.Processors
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