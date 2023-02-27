using MySqlCdc.Events;
using MySqlConnector;
using Panama.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Panama.Core.CDC.MySQL.Extensions
{
    internal static class MySqlCdcOptionsExtensions
    {
        internal static Dictionary<int, string> GetMap(this MySqlCdcOptions settings)
        {
            using (var connection = new MySqlConnection($"Server={settings.Host};User ID={settings.Username};Password={settings.Password};"))
            {
                connection.Open();

                using var command = new MySqlCommand(@"
                    
                    SELECT POS, `NAME`
                    FROM INFORMATION_SCHEMA.INNODB_COLUMNS
                    WHERE TABLE_ID = @TABLE_ID
                    ORDER BY POS;"

                , connection);

                var result = new Dictionary<int, string>();
                using var reader = command.ExecuteReader();
                while (reader.Read())
                    result.Add(reader.GetInt32(0), reader.GetString(1));

                connection.Close();

                return result;
            }
        }
    }
}
