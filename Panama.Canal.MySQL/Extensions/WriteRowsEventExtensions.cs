using Microsoft.Extensions.DependencyInjection;
using MySqlCdc.Events;
using Panama.Canal.Models.Messaging;
using Panama.Canal.MySQL.Models;
using Panama.Interfaces;
using Panama.Security.Resolvers;
using System.Text;

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

        internal static void SetValues<T>(this RowData row, T data, Dictionary<int, string> map)
            where T : IModel
        {
            for (int i = 0; i < row.Cells?.Count; i++)
            {
                if (row.Cells[i] == null)
                    continue;
                if (row.Cells[i] is byte[])
                    data.SetValue<T>(map[i], Encoding.UTF8.GetString((byte[])row.Cells[i]!));
                else
                    data.SetValue<T>(map[i], row.Cells[i]);
            }
        }

        internal static List<InternalMessage> GetOutboxMessages(this WriteRowsEvent @event, MySqlSettings settings)
        {
            var messages = new List<InternalMessage>();

            if (@event.IsEmpty())
                return messages;

            if (@event.TableId != settings.OutboxLocalTableId)
                return messages;

            foreach (var row in @event.Rows)
            {
                var message = new Outbox();

                row.SetValues(message, settings.OutboxTableMap);
                
                messages.Add(message);
            }

            return messages;
        }

        internal static List<InternalMessage> GetInboxMessages(this WriteRowsEvent @event, MySqlSettings settings)
        {
            var messages = new List<InternalMessage>();

            if (@event.IsEmpty())
                return messages;

            if (@event.TableId != settings.InboxLocalTableId)
                return messages;

            foreach (var row in @event.Rows)
            {
                var message = new Inbox();

                row.SetValues(message, settings.InboxTableMap);

                messages.Add(message);
            }

            return messages;
        }
    }
}
