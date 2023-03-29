using Panama.Extensions;
using Panama.Interfaces;

namespace Panama.Canal.Models
{
    public class InternalMessage : IModel
    {
        public InternalMessage()
        {
            if (Created == DateTime.MinValue)
                Created = DateTime.Now;
        }
        public override int GetHashCode()
        {
            if (this is null) return 0;

            var hash_Id = this._Id.GetHashCode();
            var hashId = this.Id.GetHashCode();

            return hash_Id ^ hashId;
        }
        public int _Id { get; set; }
        public string Id { get; set; } = String.Empty;
        public string CorrelationId { get; set; } = String.Empty;
        public string Version { get; set; } = String.Empty;
        public string Name { get; set; } = String.Empty;
        public string Group { get; set; } = String.Empty;
        public string Broker { get; set; } = String.Empty;
        public string Content { get; set; } = String.Empty;
        public int Retries { get; set; }
        public DateTime Created { get; set; }
        public DateTime? Expires { get; set; }
        public string Status { get; set; } = String.Empty;
        public MessageStatus StatusEnum => Status.ToEnum<MessageStatus>();
    }
}
