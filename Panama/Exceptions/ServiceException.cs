namespace Panama.Exceptions
{
    [Serializable]
    public class ServiceException : Exception
    {
        public IList<string> Messages { get; set; }

        public ServiceException(string message)
            : base(message)
        {
            if (Messages == null)
                Messages = new List<string>();

            Messages.Add(message);
        }

        public ServiceException(IEnumerable<string> messages)
            : base($"Service Exception: {string.Join("; ", messages).Trim()}.")
        {
            if (Messages == null)
                Messages = new List<string>();

            foreach (var message in messages)
                Messages.Add(message);
        }
    }
}
