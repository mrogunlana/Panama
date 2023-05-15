using Panama.Extensions;
using Panama.Interfaces;
using Panama.Models;

namespace Panama.Samples.TestApi.Commands
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
