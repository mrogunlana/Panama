using Panama.Core.Commands;
using Panama.Core.Entities;
using System.Collections.Generic;
using System.Threading;
using KeyValuePair = Panama.Core.Entities.KeyValuePair;

namespace Panama.Core.Tests.Commands
{
    public class SerialCommand2 : ICommand
    {
        public void Execute(List<IModel> data)
        {
            data.Add(new KeyValuePair("test", 2));

            Thread.Sleep(5000);
        }
    }
}
