using Panama.Core.Commands;
using Panama.Core.Entities;
using Panama.Core.Logger;
using Panama.Core.MySql.Dapper.Interfaces;
using Panama.Core.Tests.Models;

namespace Panama.Core.Tests.Commands
{
    public class InsertBatchCsvDataUsingPanamaCoreMySqlDapperLibrary : ICommand
    {
        private readonly IMySqlQuery _sql;

        public InsertBatchCsvDataUsingPanamaCoreMySqlDapperLibrary(
            IMySqlQuery sql)
        {
            _sql = sql;
        }
        public void Execute(Subject subject)
        {
            var models = subject.Context.DataGet<Csv>();
            var batch = subject.Context.KvpGetSingle<int>("Batch");

            _sql.InsertBatch(models, batch);
        }
    }
}
