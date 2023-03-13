using Panama.Interfaces;
using Panama.Models;
using System.Threading.Tasks;
using System.Threading;

namespace Panama.Tests.Commands
{
    public class SerialCommand3 : ICommand
    {
        public async Task Execute(IContext context)
        {
            context.Data.Add(new Kvp<string, int>("test", 3));
            //wait 5 seconds before next command...
            Thread.Sleep(2000);
        }
    }
}
