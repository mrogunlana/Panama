using Panama.Core.Commands;
using Panama.Core.Entities;
using Panama.Core.Sql;
using Panama.Core.Tests.Models;

namespace Panama.Core.Tests.Commands
{
    public class InsertLogEntryCommand : ICommand
    {
        private readonly IQuery _query;

        public InsertLogEntryCommand(IQuery query)
        {
            _query = query;
        }
        public void Execute(Subject subject)
        {
            var model = subject.Context.DataGetSingle<Log>();

            _query.Insert(model);
        }
    }
}
