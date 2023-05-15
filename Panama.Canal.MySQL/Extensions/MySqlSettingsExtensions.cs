using Panama.Canal.Models.Messaging;
using Panama.Canal.MySQL.Models;
using System.Text.RegularExpressions;

namespace Panama.Canal.MySQL.Extensions
{
    public static class MySqlSettingsExtensions
    {
        public static Dictionary<int, string> GetMap(this MySqlSettings settings, string table)
        {
            if (string.IsNullOrEmpty(table))
                throw new ArgumentNullException("Table name must have value to get schema map.");

            if (table.Equals(settings.PublishedTable, StringComparison.InvariantCultureIgnoreCase))
                return settings.PublishedTableMap;
            if (table.Equals(settings.ReceivedTable, StringComparison.InvariantCultureIgnoreCase))
                return settings.ReceivedTableMap;
            if (table.Equals(settings.InboxTable, StringComparison.InvariantCultureIgnoreCase))
                return settings.InboxTableMap;
            if (table.Equals(settings.OutboxTable, StringComparison.InvariantCultureIgnoreCase))
                return settings.OutboxTableMap;
            if (table.Equals(settings.SagaTable, StringComparison.InvariantCultureIgnoreCase))
                return settings.SagaTableMap;

            throw new Exception($"Table map {table} not found!");
        }

        public static InternalMessage GetModel(this MySqlSettings settings, string table)
        {
            if (string.IsNullOrEmpty(table))
                throw new ArgumentNullException("Table name must have value to get schema map.");

            if (table.Equals(settings.InboxTable, StringComparison.InvariantCultureIgnoreCase))
                return new Outbox();
            if (table.Equals(settings.OutboxTable, StringComparison.InvariantCultureIgnoreCase))
                return new Inbox();
            if (table.Equals(settings.PublishedTable, StringComparison.InvariantCultureIgnoreCase))
                return new Published();
            if (table.Equals(settings.ReceivedTable, StringComparison.InvariantCultureIgnoreCase))
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

        internal static bool IsSupportSkipLocked(this MySqlSettings settings)
        {
            switch (settings.Type)
            {
                case MySqlType.MySql when settings.Version.Major >= 8:
                case MySqlType.MariaDb when settings.Version.Major > 10:
                case MySqlType.MariaDb when settings.Version.Major == 10 && settings.Version.Minor >= 6:
                    return true;
                default:
                    return false;
            }
        }
    }
}
