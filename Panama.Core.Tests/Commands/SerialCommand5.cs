using Panama.Core.Commands;
using Panama.Core.Entities;
using System.Collections.Generic;
using System.Threading;
using KeyValuePair = Panama.Core.Entities.KeyValuePair;

namespace Panama.Core.Tests.Commands
{
    public class SerialCommand5 : ICommand
    {
        public void Execute(Subject subject)
        {
            subject.Context.Add(new KeyValuePair("test", 5));

            Thread.Sleep(5000);
        }
    }
}
