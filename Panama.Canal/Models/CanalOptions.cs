using Panama.Canal.Extensions;
using Panama.Interfaces;
using System.Reflection;

namespace Panama.Canal.Models
{
    public class CanalOptions : IModel
    {
        public string Instance { get; }
        public static string Section { get; set; } = "Panama.Canal.CanalOptions";
        public bool UseLock { get; set; }
        public string? GroupPrefix { get; set; }
        public int ProducerThreads { get; set; } = 1;
        public int ConsumerThreads { get; set; } = 1;
        public string? TopicPrefix { get; set; }
        public string Version { get; set; } = "v1";
        public string DefaultGroup { get; set; } = "panama.queue." + Assembly.GetEntryAssembly()?.GetName().Name!.ToLower();

        public int SucceedMessageExpiredAfter { get; set; }
        public int FailedMessageExpiredAfter { get; set; }
        public int FailedRetryInterval { get; set; }
        public int FailedRetryCount { get; set; }
        public int SuccessfulMessageExpiredAfter { get; set; }

        public CanalOptions()
        {
            Instance = this.GetInstance();
            SuccessfulMessageExpiredAfter = 24 * 3600;
            FailedMessageExpiredAfter = 15 * 24 * 3600;
            FailedRetryInterval = 60;
            FailedRetryCount = 50;
        }
    }
}
