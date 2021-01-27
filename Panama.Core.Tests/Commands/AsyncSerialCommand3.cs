using Panama.Core.Commands;
using System.Threading.Tasks;
using KeyValuePair = Panama.Core.Entities.KeyValuePair;

namespace Panama.Core.Tests.Commands
{
    public class AsyncSerialCommand3 : ICommandAsync
    {
        public async Task Execute(Subject subject)
        {
            await Task.Run(() => {
                subject.Context.Add(new KeyValuePair("test", 3));
            });
        }
    }
}
