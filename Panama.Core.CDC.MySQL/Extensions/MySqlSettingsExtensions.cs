using MySqlConnector;
using Panama.Core.Interfaces;
using Panama.Core.Security.Interfaces;
using System.Data.Common;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Panama.Core.CDC.MySQL.Extensions
{
    internal static class MySqlSettingsExtensions
    {
        internal static Dictionary<int, string> GetMap(this MySqlSettings settings, string table)
        {
            if (string.IsNullOrEmpty(table))
                throw new ArgumentNullException("Table name must have value to get schema map.");

            if (table.Equals(nameof(settings.PublishedTableMap), StringComparison.InvariantCultureIgnoreCase))
                return settings.PublishedTableMap;
            if (table.Equals(nameof(settings.ReceivedTableMap), StringComparison.InvariantCultureIgnoreCase))
                return settings.ReceivedTableMap;

            throw new Exception($"Table map {table} not found!");
        }

        internal static _Message GetModel(this MySqlSettings settings, string table)
        {
            if (string.IsNullOrEmpty(table))
                throw new ArgumentNullException("Table name must have value to get schema map.");

            if (table.Equals(nameof(settings.PublishedTableMap), StringComparison.InvariantCultureIgnoreCase))
                return new Published();
            if (table.Equals(nameof(settings.ReceivedTableMap), StringComparison.InvariantCultureIgnoreCase))
                return new Received();

            throw new Exception($"Table map {table} not found!");
        }

        internal static void SetVersion(this MySqlSettings settings, string value)
        {
            var regex = new Regex("\\d+\\.\\d+\\.?(?:\\d+)?");
            var id = "MariaDb".ToLowerInvariant();
            var list = regex.Matches(value);
            if (list.Count == 0)
                return;

            var type = (MySqlType)(value.ToLower().Contains(id) ? 1 : 0);
            var version = type != MySqlType.MariaDb || list.Count <= 1
                ? Version.Parse(list[0].Value)
                : Version.Parse(list[1].Value);

            settings.Version = version;
            settings.Type = type;
        }
    }
}
