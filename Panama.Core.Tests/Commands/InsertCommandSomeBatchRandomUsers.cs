﻿using Panama.Core.Commands;
using Panama.Core.MySql.Dapper.Interfaces;
using Panama.Core.MySql.Dapper.Models;
using Panama.Core.Tests.Models;
using System.Collections.Generic;

namespace Panama.Core.Tests.Commands
{
    public class InsertCommandSomeBatchRandomUsers : ICommand
    {
        private readonly IMySqlQuery _query;

        public InsertCommandSomeBatchRandomUsers(IMySqlQuery query)
        {
            _query = query;
        }
        public void Execute(Subject subject)
        {
            var list = new List<User>() {
                new User() {
                    ID = System.Guid.NewGuid(),
                    Email = $"test.{System.Guid.NewGuid().ToString().Replace("-", "")}@test.com",
                    FirstName = "test1",
                    LastName = "test"
                },
                new User() {
                    ID = System.Guid.NewGuid(),
                    Email = $"test.{System.Guid.NewGuid().ToString().Replace("-", "")}@test.com",
                    FirstName = "test2",
                    LastName = "test"
                }
            };
            var definition = new Definition();

            definition.Token = subject.Token;

            _query.InsertBatch(list);
        }
    }
}
