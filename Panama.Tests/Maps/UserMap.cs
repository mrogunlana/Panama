using DapperExtensions.Mapper;
using Panama.Tests.Models;

namespace Panama.Tests.Maps
{
    public class UserMap : ClassMapper<User>
    {
        public UserMap()
        {
            Table("[User]");

            Map(x => x._ID).Key(KeyType.Identity);
            Map(x => x.TestValue).ReadOnly();

            AutoMap();
        }
    }
}
