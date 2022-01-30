using DapperExtensions.Mapper;
using Panama.Core.Tests.Models;

namespace Panama.Core.Tests.Maps
{
    public class CsvMap : ClassMapper<Csv>
    {
        public CsvMap()
        {
            Table("Csv");

            Map(x => x._ID).Key(KeyType.Identity);
            Map(x => x.ID).Key(KeyType.NotAKey);

            AutoMap();
        }
    }
}
