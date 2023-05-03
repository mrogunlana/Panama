using Panama.Interfaces;
using System;
using System.Threading.Tasks;

namespace Panama.Canal.Tests.MySQL.Commands
{
    public class ThrowException : ICommand
    {
        public Task Execute(IContext context)
        {
            throw new NotImplementedException();
        }
    }
}
