using Panama.Core.Commands;
using Panama.Core.Entities;
using System.Collections.Generic;
using System.Threading;
using KeyValuePair = Panama.Core.Entities.KeyValuePair;

namespace Panama.Core.Tests.Commands
{
    public class SerialCommand1 : ICommand
    {
        public void Execute(List<IModel> data)
        {
            data.Add(new KeyValuePair("test", 1));

            //wait 5 seconds before next command...
            Thread.Sleep(2000);
        }
    }
}
