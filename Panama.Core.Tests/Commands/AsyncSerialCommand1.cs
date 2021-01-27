using Panama.Core.Commands;
using System.Threading;
using System.Threading.Tasks;
using KeyValuePair = Panama.Core.Entities.KeyValuePair;

namespace Panama.Core.Tests.Commands
{
    public class AsyncSerialCommand1 : ICommandAsync
    {
        public async Task Execute(Subject subject)
        {
            await Task.Run(() => {
                subject.Context.Add(new KeyValuePair("test", 1));

                //wait 5 seconds before next command...
                Thread.Sleep(2000);
            });
        }
    }
}
