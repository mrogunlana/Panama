using DapperExtensions;
using Panama.Commands;
using Panama.Entities;
using Panama.MySql.Dapper.Interfaces;
using Panama.MySql.Dapper.Models;
using Panama.Tests.Models;
using System;

namespace Panama.Tests.Commands
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
