﻿using DapperExtensions;
using Panama.Core.Commands;
using Panama.Core.Entities;
using Panama.Core.MySql.Dapper.Interfaces;
using Panama.Core.MySql.Dapper.Models;
using Panama.Core.Tests.Models;

namespace Panama.Core.Tests.Commands
{
    public class UpdateCommand : ICommand
    {
        private readonly IMySqlQuery _query;

        public UpdateCommand(IMySqlQuery query)
        {
            _query = query;
        }
        public void Execute(Subject subject)
        {
            var user = subject.Context.DataGetSingle<User>();
            var definition = new Definition();

            definition.Token = subject.Token;
            definition.Predicate = Predicates.Field<User>(x => x._ID, Operator.Eq, user._ID);

            _query.Update(user);
        }
    }
}
