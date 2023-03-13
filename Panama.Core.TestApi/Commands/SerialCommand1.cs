using Panama.Core.Extensions;
using Panama.Core.Interfaces;
using Panama.Core.Models;

namespace Panama.Core.Tests.Commands
{
    public class SerialCommand1 : ICommand
    {
        public async Task Execute(IContext context)
        {
            var test = new Kvp<string, int>("test", 1);

            context.Data.Add(test);
            context.Data.Snapshot(test);

            //wait 5 seconds before next command...
            await Task.Delay(2000);
        }
    }
}
