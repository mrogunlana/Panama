using Panama.Canal.Attributes;
using Panama.Canal.RabbitMQ.Models;

namespace Panama.Canal.RabbitMQ.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class RabbitTopicAttribute : TopicAttribute
    {
        public RabbitTopicAttribute(string name)
            : base (name, null, typeof(RabbitMQTarget)) { }
        public RabbitTopicAttribute(string name, string group)
            : base (name, group, typeof(RabbitMQTarget)) { }
    }
}
