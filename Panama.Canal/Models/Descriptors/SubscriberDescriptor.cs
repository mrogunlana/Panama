using Panama.Canal.Interfaces;

namespace Panama.Canal.Models.Descriptors
{
    public class SubscriberDescriptor : IDescriptor
    {
        public string Topic { get; set; }
        public string Group { get; set; }
        public Type Implementation { get; set; }
        public Type Target { get; set; }

        public SubscriberDescriptor(
              string topic
            , string group
            , Type subscriber
            , Type target)
        {
            Topic = topic;
            Group = group;
            Implementation = subscriber;
            Target = target;
        }
    }
}
