using Panama.Commands;
using Panama.Entities;
using Panama.MySql.Dapper.Interfaces;
using Panama.MySql.Dapper.Models;
using Panama.Tests.Models;

namespace Panama.Tests.Commands
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
