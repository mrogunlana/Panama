using Panama.Interfaces;

namespace Panama.Canal.Models
{
    public class Job : IModel 
    {
        public Job(Type type
            , string expression
            , string? group = null
            , bool active = true)
        {
            Type = type;
            Group = group;
            Active = active;
            CronExpression = expression;
        }

        public Type Type { get; }
        public string CronExpression { get; }
        public string? Group { get; }
        public bool Active { get; set; }
    }
}
