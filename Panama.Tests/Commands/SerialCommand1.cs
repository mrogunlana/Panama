﻿using Panama.Interfaces;
using Panama.Models;
using System.Threading;
using System.Threading.Tasks;

namespace Panama.Tests.Commands
{
    public class SerialCommand1 : ICommand
    {
        public async Task Execute(IContext context)
        {
            context.Data.Add(new Kvp<string, int>("test", 1));
            //wait 5 seconds before next command...
            Thread.Sleep(2000);
        }
    }
}