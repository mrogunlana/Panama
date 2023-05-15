using Panama.Extensions;
using Panama.Interfaces;

namespace Panama.Canal.Models.Messaging
{
    public class InternalMessage : IModel
    {
        public InternalMessage()
        {
            if (Created == DateTime.MinValue)
                Created = DateTime.UtcNow;
        }
        public override int GetHashCode()
        {
            if (this is null) return 0;

            var hash_Id = _Id.GetHashCode();
            var hashId = Id.GetHashCode();

            return hash_Id ^ hashId;
        }
        public long _Id { get; set; }
        public string Id { get; set; } = string.Empty;
        public string CorrelationId { get; set; } = string.Empty;
        public string? Version { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Group { get; set; } = string.Empty;
        public string Broker { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public int Retries { get; set; }
        public DateTime Created { get; set; }
        public DateTime? Expires { get; set; }
        public string Status { get; set; } = string.Empty;
        public MessageStatus StatusEnum => Status.ToEnum<MessageStatus>();
    }
}
