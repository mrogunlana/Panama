﻿using Panama.Core.Commands;
using Panama.Core.Entities;
using Panama.Core.MySql.Dapper.Interfaces;
using Panama.Core.MySql.Dapper.Models;
using Panama.Core.Tests.Models;

namespace Panama.Core.Tests.Commands
{
    public class SelectUserByEmailCommand : ICommand
    {
        private readonly IMySqlQuery _query;

        public SelectUserByEmailCommand(IMySqlQuery query)
        {
            _query = query;
        }
        public void Execute(Subject subject)
        {
            var user = subject.Context.DataGetSingle<User>();
            var definition = new Definition();

            definition.Sql = "select u.* from User u where u.Email = @Email;";
            definition.Parameters = new { user.Email };
            definition.Token = subject.Token;

            var result = _query.GetSingle<User>(definition);

            subject.Context.Remove(user);
            subject.Context.Add(result);
        }
    }
}