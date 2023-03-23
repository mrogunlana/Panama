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

        public CanalOptions()
        {
            Instance = this.GetInstance();
        }
    }
}
