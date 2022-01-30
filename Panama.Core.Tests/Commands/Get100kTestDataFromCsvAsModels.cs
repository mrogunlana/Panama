using Panama.Core.Commands;
using Panama.Core.Entities;
using Panama.Core.MySql.Dapper.Interfaces;
using Panama.Core.MySql.Dapper.Models;
using Panama.Core.Tests.Models;
using System.Threading.Tasks;

namespace Panama.Core.Tests.Commands
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
