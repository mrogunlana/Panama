using DapperExtensions.Mapper;
using Panama.Core.Tests.Models;

namespace Panama.Core.Tests.Maps
{
    public class UserMap : ClassMapper<User>
    {
        public UserMap()
        {
            Table("User");

            Map(x => x._ID).Key(KeyType.Identity);
            Map(x => x.Created).ReadOnly();
            Map(x => x.ConfirmPassword).Ignore();

            AutoMap();
        }
    }
}
