using Panama.Canal.Interfaces;
using Panama.Interfaces;

namespace Panama.Canal.Models
{
    public class Subscription : IModel
    {
        public Type Subscriber { get; set; }
        public string Topic { get; set; }
        public string Group { get; set; }
        public ITarget? Broker { get; set; }

        public Subscription(
              string topic
            , string group
            , Type subscriber
            , ITarget? broker = null)
        {
            Topic = topic;
            Group = group;
            Subscriber = subscriber;
            Broker = broker;
        }
    }
}
