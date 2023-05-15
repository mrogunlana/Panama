using Panama.Canal.Extensions;
using Panama.Models.Options;
using System.Reflection;

namespace Panama.Canal.Models.Options
{
    public class CanalOptions : OptionBuilder
    {
        public string Instance { get; }
        public bool UseLock { get; set; }
        public string? GroupPrefix { get; set; }
        public int ProducerThreads { get; set; } = 1;
        public int ConsumerThreads { get; set; } = 1;
        public string? TopicPrefix { get; set; }
        public string? Scope { get; set; }
        public string Version { get; set; } = "v1";
        public string DefaultGroup { get; set; } = "panama.queue." + Assembly.GetEntryAssembly()?.GetName().Name!.ToLower();

        public int SuccessfulMessageExpiredAfter { get; set; }
        public int FailedMessageExpiredAfter { get; set; }
        public int FailedRetryInterval { get; set; }
        public int FailedRetryCount { get; set; }

        public CanalOptions()
            : base()
        {
            Instance = this.GetInstance();
            SuccessfulMessageExpiredAfter = 24 * 3600;  //seconds = 1 day
            FailedMessageExpiredAfter = 15 * 24 * 3600; //seconds = 15 days
            FailedRetryInterval = 60;
            FailedRetryCount = 50;
        }
    }
}
