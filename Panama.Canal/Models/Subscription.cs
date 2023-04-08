using Panama.Interfaces;

namespace Panama.Canal.Models
{
    public class Subscription : IModel
    {
        public string Topic { get; set; }
        public string Group { get; set; }
        public Type Subscriber { get; set; }
        public Type Target { get; set; }

        public Subscription(
              string topic
            , string group
            , Type subscriber
            , Type target)
        {
            Topic = topic;
            Group = group;
            Subscriber = subscriber;
            Target = target;
        }
    }
}
