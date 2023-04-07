using MySqlCdc.Events;
using Panama.Canal.Models;
using Panama.Canal.MySQL.Models;

namespace Panama.Canal.MySQL.Extensions
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

        internal static List<InternalMessage> GetOutboxMessages(this WriteRowsEvent @event, MySqlSettings settings)
        {
            var messages = new List<InternalMessage>();

            if (@event.IsEmpty())
                return messages;

            if (@event.TableId != settings.OutboxTableId)
                return messages;

            foreach (var row in @event.Rows)
            {
                var message = new Outbox();

                for (int i = 0; i < row.Cells?.Count; i++)
                    message.SetValue<Outbox>(settings.OutboxTableMap[i], row.Cells[i]);

                messages.Add(message);
            }

            return messages;
        }
        
        internal static List<InternalMessage> GetInboxMessages(this WriteRowsEvent @event, MySqlSettings settings)
        {
            var messages = new List<InternalMessage>();

            if (@event.IsEmpty())
                return messages;

            if (@event.TableId != settings.InboxTableId)
                return messages;

            foreach (var row in @event.Rows)
            {
                var message = new Inbox();

                for (int i = 0; i < row.Cells?.Count; i++)
                    message.SetValue<Inbox>(settings.InboxTableMap[i], row.Cells[i]);

                messages.Add(message);
            }

            return messages;
        }
    }
}
