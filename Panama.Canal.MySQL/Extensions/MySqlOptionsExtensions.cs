using Panama.Canal.MySQL.Models;

namespace Panama.Canal.MySQL.Extensions
{
    public static class MySqlOptionsExtensions
    {
        public static string GetConnectionString(this MySqlOptions options)
        {
            return $"Server={options.Host};Port={options.Port};Database={options.Database};Uid={options.Username};Pwd={options.Password};";
        }
    }
}