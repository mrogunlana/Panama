using MySqlConnector;

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

                command.Parameters.Add("@TABLE_ID", MySqlDbType.Int32);

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
