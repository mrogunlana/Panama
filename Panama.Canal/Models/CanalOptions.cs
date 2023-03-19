using Panama.Canal.Extensions;
using Panama.Interfaces;

namespace Panama.Canal.Models
{
    public class CanalOptions : IModel
    {
        public string Instance { get; }
        public static string Section { get; set; } = "Panama.Canal.CanalOptions";
        public bool UseLock { get; set; }
        public int Version { get; set; } = 1;

        public CanalOptions()
        {
            Instance = this.GetInstance();
        }
    }
}
