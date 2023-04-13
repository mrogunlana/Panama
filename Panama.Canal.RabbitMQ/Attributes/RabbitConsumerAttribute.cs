using Panama.Canal.Attributes;
using Panama.Canal.RabbitMQ.Models;

namespace Panama.Canal.RabbitMQ.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class RabbitConsumerAttribute : TopicAttribute
    {
        public RabbitConsumerAttribute(string name)
            : base (name, string.Empty, typeof(RabbitMQTarget)) { }
        public RabbitConsumerAttribute(string name, string group)
            : base (name, group, typeof(RabbitMQTarget)) { }
    }
}
