namespace Panama.Canal.Exceptions
{
    [Serializable]
    public class SubscriptionException : Exception
    {
        public IList<string> Messages { get; set; }

        public SubscriptionException(string message)
            : base(message)
        {
            if (Messages == null)
                Messages = new List<string>();

            Messages.Add(message);
        }

        public SubscriptionException(string message, Exception inner)
            : base(message, inner)
        {
            if (Messages == null)
                Messages = new List<string>();

            Messages.Add(message);
        }

        public SubscriptionException(IEnumerable<string> messages)
            : base($"Subscription Exception: {string.Join("; ", messages).Trim()}.")
        {
            if (Messages == null)
                Messages = new List<string>();

            foreach (var message in messages)
                Messages.Add(message);
        }
    }
}
