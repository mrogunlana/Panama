using Panama.Commands;
using Panama.Entities;
using Panama.MySql.Dapper.Interfaces;
using Panama.MySql.Dapper.Models;
using Panama.Tests.Models;
using System.Threading.Tasks;

namespace Panama.Tests.Commands
{
    public class Get100kTestDataFromCsvAsModels : ICommandAsync
    {
        private readonly ICsvClient _csv;

        public Get100kTestDataFromCsvAsModels(ICsvClient csv)
        {
            _csv = csv;
        }
        public async Task Execute(Subject subject)
        {
            var filename = subject.Context.KvpGetSingle<string>("Filename");

            subject.Context.AddRange(await _csv.Get<Csv>(filename));
        }
    }
}
