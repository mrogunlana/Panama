using Panama.Core.Interfaces;
using Panama.Core.Models;
using System.Threading;
using System.Threading.Tasks;

namespace Panama.Core.Tests.Commands
{
    public class SerialCommand2 : ICommand
    {
        public async Task Execute(IContext context)
        {
            context.Data.Add(new Kvp<string, int>("test", 2));
            //wait 5 seconds before next command...
            Thread.Sleep(2000);
        }
    }
}
