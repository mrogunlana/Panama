using Panama.Canal.Jobs;
using Panama.Models.Options;

namespace Panama.Canal.Models.Options
{
    public class SchedulerOptions : OptionBuilder
    {
        public IList<Type> Removed { get; set; }
        public IList<Job> Added { get; set; }
        public IList<Job> Current { get; set; }
        public SchedulerOptions()
            : base() 
        { 
            Added = new List<Job>();
            Removed = new List<Type>();
            Current = new List<Job> {
                new Job(
                    type: typeof(DelayedPublished),
                    expression: "0 * * * * ?"),
                new Job(
                    type: typeof(PublishedRetry),
                    expression: "0 * * * * ?"),
                new Job(
                    type: typeof(ReceivedRetry),
                    expression: "0 * * * * ?"),
                new Job(
                    type: typeof(DeleteExpired),
                    expression: "0 */5 * * * ?")
            };
        }
    }
}