using MySqlCdc.Events;

namespace Panama.Core.CDC.MySQL.Extensions
{
    internal static class WriteRowsEventExtensions
    {
        internal static bool IsEmpty(this WriteRowsEvent @event, int tableId)
        {
            if (@event == null)
                return true;
            if (@event.Rows == null)
                return true;
            if (@event.TableId != tableId)
                return true;

            return false;
        }

        internal static List<Outbox> GetMessages(this WriteRowsEvent @event, MySqlCdcOptions settings, Dictionary<int, string> map)
        {
            var messages = new List<Outbox>();

            if (@event.IsEmpty(settings.OutboxTableId))
                return messages;

            foreach (var row in @event.Rows)
            {
                var message = new Outbox();

                for (int i = 0; i < row.Cells?.Count; i++)
                    message.SetValue<Outbox>(map[i], row.Cells[0]);
            }

            return messages;
        }
    }
}
