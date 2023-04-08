using Panama.Canal.Interfaces;

namespace Panama.Canal.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class TopicAttribute : Attribute
    {
        public string? Group { get; set; }
        public string Name { get; } = string.Empty;
        public ITarget? Target { get; }

        public TopicAttribute(string name)
        {
            Name = name;
        }
        public TopicAttribute(string name, string group)
            : this (name)
        {
            Group = group;
        }
        public TopicAttribute(string name, string group, object broker)
            : this(name, group)
        {
            if (broker == null)
                return;
            if (broker is not ITarget)
                throw new InvalidOperationException($"Target {broker?.GetType().Name} must be type ITarget.");

            Target = broker as ITarget;
        }
    }
}
