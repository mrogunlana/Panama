using Panama.Canal.Interfaces;

namespace Panama.Canal.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class TopicAttribute : Attribute
    {
        public string? Group { get; set; }
        public string Name { get; } = string.Empty;
        public Type? Target { get; }

        public TopicAttribute(string name)
        {
            Name = name;
        }
        public TopicAttribute(string name, string? group)
            : this (name)
        {
            Group = group;
        }
        public TopicAttribute(string name, string? group, Type broker)
            : this(name, group)
        {
            if (broker == null)
                return;
            
            Target = broker;
        }
    }
}
