using Panama.Interfaces;
using Panama.Models;
using System.Threading.Tasks;
using System.Transactions;

namespace Panama.Canal.Tests.MySQL.Commands
{
    public class VerifyAmbientTransaction : ICommand
    {
        public Task Execute(IContext context)
        {
            var result = true;

            var transaction = Transaction.Current;
            if (transaction == null)
                result = false;

            context.Data.Add(new Kvp<string,bool>("HasTransaction", result));

            return Task.CompletedTask;
        }
    }
}
