using Panama.Core.CDC.Interfaces;

namespace Panama.Core.CDC.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class BrokerAttribute : Attribute
    {
        public string Group { get; set; } = string.Empty;
        public string Name { get; } = string.Empty;
        public string Topic { get; }
        public ITarget Broker { get; }

        public BrokerAttribute(ITarget broker
            , string topic)
        {
            Broker = broker;
            Topic = topic;
        }

        public BrokerAttribute(ITarget broker
            , string topic
            , string name)
        {
            Broker = broker;
            Topic = topic;
            Name = name;
        }

        public BrokerAttribute(ITarget broker
            , string topic
            , string name
            , string group)
        {
            Broker = broker;
            Topic = topic;
            Name = name;
            Group = group;
        }
    }
}
