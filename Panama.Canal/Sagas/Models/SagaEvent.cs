using Panama.Interfaces;

namespace Panama.Canal.Sagas.Models
{
    public class SagaEvent : IModel
    {
        public int _Id { get; set; }
        public string Id { get; set; }
        public string? CorrelationId { get; set; }
        public string? Content { get; set; }
        public string Trigger { get; set; } = string.Empty;
        public string Source { get; set; } = string.Empty;
        public string Destination { get; set; } = string.Empty;
        public DateTime Created { get; set; }
        public DateTime? Expires { get; set; }

        public SagaEvent()
        {
            if (string.IsNullOrEmpty(Id))
                Id = Guid.NewGuid().ToString();
            if (Created == DateTime.MinValue)
                Created = DateTime.UtcNow;
        }
    }
}
