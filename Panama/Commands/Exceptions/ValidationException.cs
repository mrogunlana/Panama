using System;
using System.Collections.Generic;

namespace Panama.Core.Commands
{
    [Serializable]
    public class ValidationException : Exception
    {
        public IList<string> Messages { get; set; }

        public ValidationException(string message)
            : base(message)
        {
            if (Messages == null)
                Messages = new List<string>();

            Messages.Add(message);
        }

        public ValidationException(IEnumerable<string> messages)
            : base($"Validation Exceptions: {string.Join("; ", messages).Trim()}.")
        {
            if (Messages == null)
                Messages = new List<string>();

            foreach (var message in messages)
                Messages.Add(message);
        }
    }
}
