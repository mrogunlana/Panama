namespace Panama.Canal.Exceptions
{
    [Serializable]
    public class BrokerException : Exception
    {
        public IList<string> Messages { get; set; }

        public BrokerException(string message)
            : base(message)
        {
            if (Messages == null)
                Messages = new List<string>();

            Messages.Add(message);
        }

        public BrokerException(string message, Exception inner)
            : base(message, inner)
        {
            if (Messages == null)
                Messages = new List<string>();

            Messages.Add(message);
        }

        public BrokerException(IEnumerable<string> messages)
            : base($"Broker Exception: {string.Join("; ", messages).Trim()}.")
        {
            if (Messages == null)
                Messages = new List<string>();

            foreach (var message in messages)
                Messages.Add(message);
        }
    }
}
