using Panama.Canal.Interfaces;

namespace Panama.Canal.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class TopicAttribute : Attribute
    {
        public string Group { get; set; } = string.Empty;
        public string Name { get; } = string.Empty;
        public string Topic { get; }
        public ITarget? Broker { get; }

        public TopicAttribute(string topic
            , ITarget? broker = null)
        {
            Broker = broker;
            Topic = topic;

        }

        public TopicAttribute(string topic
            , string name
            , ITarget? broker = null)
            : this(topic, broker)
        {
            Name = name;
        }

        public TopicAttribute(string topic
            , string name
            , string group
            , ITarget? broker)
            : this(topic, name, broker)
        {
            Group = group;
        }
    }
}
