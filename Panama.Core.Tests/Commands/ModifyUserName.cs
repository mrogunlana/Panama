using DapperExtensions;
using Panama.Core.Commands;
using Panama.Core.Entities;
using Panama.Core.MySql.Dapper.Interfaces;
using Panama.Core.MySql.Dapper.Models;
using Panama.Core.Tests.Models;
using System;

namespace Panama.Core.Tests.Commands
{
    public class ModifyUserName : ICommand
    {
        public void Execute(Subject subject)
        {
            var user = subject.Context.DataGetSingle<User>();

            user.FirstName = $"Modified: {DateTime.Now.ToLongTimeString()}";
        }
    }
}
