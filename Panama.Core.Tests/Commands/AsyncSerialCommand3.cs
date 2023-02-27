﻿using Panama.Core.Interfaces;
using Panama.Core.Models;
using System.Threading;
using System.Threading.Tasks;

namespace Panama.Core.Tests.Commands
{
    public class AsyncSerialCommand3 : ICommand
    {
        public async Task Execute(IContext context)
        {
            await Task.Run(() => {
                context.Data.Add(new Kvp<string, int>("test", 3));

                //wait 5 seconds before next command...
                Thread.Sleep(2000);
            });
        }
    }
}
