using Panama.Core.Interfaces;

namespace Panama.Core.CDC.Models
{
    public class Message : IModel 
    {
        public Message()
        {
            Headers = new Dictionary<string, string?>();
        }

        public Message(IDictionary<string, string?> headers, object? value)
        {
            Headers = headers ?? throw new ArgumentNullException(nameof(headers));
            Value = value;
        }

        public IDictionary<string, string?> Headers { get; set; }

        public object? Value { get; set; }
    }
}
