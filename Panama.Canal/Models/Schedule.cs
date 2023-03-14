using Panama.Interfaces;

namespace Panama.Canal.Models
{
    public class Schedule : IModel 
    {
        public Schedule(Type job
            , string expression
            , string? group = null
            , bool active = true)
        {
            JobType = job;
            Group = group;
            Active = active;
            CronExpression = expression;
        }

        public Type JobType { get; }
        public string CronExpression { get; }
        public string? Group { get; }
        public bool Active { get; set; }
    }
}
