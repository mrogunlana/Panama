using Panama.Commands;
using Panama.Entities;
using Panama.MySql.Dapper.Interfaces;
using Panama.MySql.Dapper.Models;
using Panama.Tests.Models;
using System.Text;

namespace Panama.Tests.Commands
{
    public class InsertCommandWithRunTimeException : ICommand
    {
        private readonly IMySqlQuery _query;

        public InsertCommandWithRunTimeException(IMySqlQuery query)
        {
            _query = query;
        }
        public void Execute(Subject subject)
        {
            var user = subject.Context.DataGetSingle<User>();
            var definition = new Definition();

            definition.Token = subject.Token;

            var builder = new StringBuilder();

            builder.Append("insert into User (ID, FirstName) ");
            builder.Append("value (@ID, 1); ");

            _query.Execute(builder.ToString(), new {
                ID = System.Guid.NewGuid()
            });
        }
    }
}
