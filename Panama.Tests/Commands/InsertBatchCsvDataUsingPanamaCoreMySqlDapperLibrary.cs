using Panama.Commands;
using Panama.Entities;
using Panama.Logger;
using Panama.MySql.Dapper.Interfaces;
using Panama.Tests.Models;

namespace Panama.Tests.Commands
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
