using Panama.Interfaces;

namespace Panama.Canal.Models.Messaging
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
