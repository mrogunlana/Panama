using DapperExtensions.Mapper;
using Panama.Tests.Models;

namespace Panama.Tests.Maps
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
