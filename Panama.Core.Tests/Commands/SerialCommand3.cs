using Panama.Core.Commands;
using Panama.Core.Entities;
using System.Collections.Generic;
using KeyValuePair = Panama.Core.Entities.KeyValuePair;

namespace Panama.Core.Tests.Commands
{
    public class SerialCommand3 : ICommand
    {
        public void Execute(Subject subject)
        {
            subject.Context.Add(new KeyValuePair("test", 3));
        }
    }
}
