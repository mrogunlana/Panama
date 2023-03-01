using Panama.Core.CDC.Interfaces;
using Panama.Core.Interfaces;

namespace Panama.Core.CDC.MySQL.Processors
{
    public class DeleteExpiredProcessor : IProcess
    {
        public DeleteExpiredProcessor(IServiceProvider provider)
        {
            
        }

        public async Task Invoke(IContext context)
        {
        }
    }
}