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
        public int Version { get; set; } = 1;
        public string DefaultGroupName { get; set; } = "panama.queue." + Assembly.GetEntryAssembly()?.GetName().Name!.ToLower();

        public CanalOptions()
        {
            Instance = this.GetInstance();
        }
    }
}
