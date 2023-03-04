using MySqlCdc.Events;
using Panama.Core.Interfaces;

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

        internal static List<_Message> GetPublishedMessages(this WriteRowsEvent @event, MySqlCdcOptions settings, Dictionary<int, string> map)
        {
            var messages = new List<_Message>();

            if (@event.IsEmpty())
                return messages;

            if (@event.TableId != settings.PublishedTableId)
                return messages;

            foreach (var row in @event.Rows)
            {
                var message = new Published();

                for (int i = 0; i < row.Cells?.Count; i++)
                    message.SetValue<Published>(map[i], row.Cells[0]);

                messages.Add(message);
            }

            return messages;
        }
        
        internal static List<_Message> GetReceivedMessages(this WriteRowsEvent @event, MySqlCdcOptions settings, Dictionary<int, string> map)
        {
            var messages = new List<_Message>();

            if (@event.IsEmpty())
                return messages;

            if (@event.TableId != settings.ReceivedTableId)
                return messages;

            foreach (var row in @event.Rows)
            {
                var message = new Received();

                for (int i = 0; i < row.Cells?.Count; i++)
                    message.SetValue<Received>(map[i], row.Cells[0]);

                messages.Add(message);
            }

            return messages;
        }
    }
}
