using CsvHelper;
using Panama.Logger;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Panama.Tests
{
    public class MockCsvClient : ICsvClient
    {
        private readonly ILog _log;
        public MockCsvClient(ILog log)
        {
            _log = log;
        }

        public async Task<IEnumerable<T>> Get<T>(string key)
        {
            using (var stream = File.OpenRead(key))
            using (var reader = new StreamReader(stream))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                return await Task.FromResult(csv.GetRecords<T>().ToList());
        }

        public async Task Save<T>(string key, List<T> models)
        {
            using (var stream = new FileStream($@"csv\{key.Replace("/", "")}", FileMode.OpenOrCreate))
            using (var writer = new StreamWriter(stream, Encoding.UTF8))
            using (var csv = new CsvWriter(writer, CultureInfo.CurrentCulture, false))
            {
                await csv.WriteRecordsAsync(models);

                writer.Flush();
            }
        }
    }
}
