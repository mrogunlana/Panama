using MySqlConnector;
using Panama.Core.Interfaces;
using Panama.Core.Security.Interfaces;
using System.Data.Common;
using System.Reflection;

namespace Panama.Core.CDC.MySQL.Extensions
{
    internal static class MessageExtensions
    {
        internal static void ResetMessageMetadata(this _Message message)
        {
            if (message == null)
                return;

            message._Id = 0;
            message.Status = MessageStatus.Queued.ToString();
            message.Expires = null;
            message.Created = DateTime.UtcNow;
            message.Retries = 0;
        }
    }
}
