using Panama.Core.Commands;
using Panama.Core.Entities;
using System.Collections.Generic;
using System.Threading;
using KeyValuePair = Panama.Core.Entities.KeyValuePair;

namespace Panama.Core.Tests.Commands
{
    public class SerialCommand4 : ICommand
    {
        public void Execute(Subject subject)
        {
            subject.Context.Add(new KeyValuePair("test", 4));

            //wait 5 seconds before next command...
            Thread.Sleep(2000);
        }
    }
}
