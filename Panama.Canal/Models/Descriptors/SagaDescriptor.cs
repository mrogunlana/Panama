using Panama.Canal.Interfaces;

namespace Panama.Canal.Models.Descriptors
{
    public class SagaDescriptor : IDescriptor
    {
        public string Topic { get; set; }
        public string Group { get; set; }
        public Type Implementation { get; set; }
        public Type Target { get; set; }

        public SagaDescriptor(
              string topic
            , string group
            , Type saga
            , Type target)
        {
            Topic = topic;
            Group = group;
            Implementation = saga;
            Target = target;
        }
    }
}
