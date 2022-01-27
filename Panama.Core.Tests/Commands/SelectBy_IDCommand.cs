using Panama.Core.Commands;
using Panama.Core.Entities;
using Panama.Core.MySql.Dapper.Interfaces;
using Panama.Core.MySql.Dapper.Models;
using Panama.Core.Tests.Models;

namespace Panama.Core.Tests.Commands
{
    public class SelectBy_IDCommand : ICommand
    {
        private readonly IMySqlQuery _query;

        public SelectBy_IDCommand(IMySqlQuery query)
        {
            _query = query;
        }
        public void Execute(Subject subject)
        {
            var user = subject.Context.DataGetSingle<User>();
            var ID = user?._ID;
            if (ID == null)
                ID = subject.Context.KvpGetSingle<int?>("_ID");

            var definition = new Definition();

            definition.Sql = "select u.* from User u where u._ID = @ID;";
            definition.Parameters = new { ID };
            definition.Token = subject.Token;

            var result = _query.GetSingle<User>(definition);

            subject.Context.Remove(user);
            subject.Context.Add(result);
        }
    }
}
