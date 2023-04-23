using Panama.Canal.Interfaces;
using Panama.Canal.Models;

namespace Panama.Canal.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class DefaultTopicAttribute : TopicAttribute
    {
        public DefaultTopicAttribute(string name)
            : base(name, null, typeof(DefaultTarget)) { }
        public DefaultTopicAttribute(string name, string group)
            : base (name, group, typeof(DefaultTarget)) { }
    }
}
