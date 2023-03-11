using Panama.Core.CDC.Models;

namespace Panama.Core.CDC.MySQL.Extensions
{
    internal static class MessageExtensions
    {
        internal static void ResetMessageMetadata(this InternalMessage message)
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
