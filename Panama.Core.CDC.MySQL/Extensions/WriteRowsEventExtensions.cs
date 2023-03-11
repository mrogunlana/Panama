using MySqlCdc.Events;
using Panama.Core.CDC.Models;

namespace Panama.Core.CDC.MySQL.Extensions
{
    internal static class WriteRowsEventExtensions
    {
        internal static bool IsEmpty(this WriteRowsEvent @event)
        {
            if (@event == null)
                return true;
            if (@event.Rows == null)
                return true;

            return false;
        }

        internal static List<InternalMessage> GetPublishedMessages(this WriteRowsEvent @event, MySqlSettings settings)
        {
            var messages = new List<InternalMessage>();

            if (@event.IsEmpty())
                return messages;

            if (@event.TableId != settings.PublishedTableId)
                return messages;

            foreach (var row in @event.Rows)
            {
                var message = new Published();

                for (int i = 0; i < row.Cells?.Count; i++)
                    message.SetValue<Published>(settings.PublishedTableMap[i], row.Cells[i]);

                messages.Add(message);
            }

            return messages;
        }
        
        internal static List<InternalMessage> GetReceivedMessages(this WriteRowsEvent @event, MySqlSettings settings)
        {
            var messages = new List<InternalMessage>();

            if (@event.IsEmpty())
                return messages;

            if (@event.TableId != settings.ReceivedTableId)
                return messages;

            foreach (var row in @event.Rows)
            {
                var message = new Received();

                for (int i = 0; i < row.Cells?.Count; i++)
                    message.SetValue<Received>(settings.ReceivedTableMap[i], row.Cells[i]);

                messages.Add(message);
            }

            return messages;
        }
    }
}
