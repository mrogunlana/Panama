using Panama.Interfaces;

namespace Panama.Canal.MySQL.Models
{
    public class MySqlOptions : IModel
    {
        public static string Section { get; set; } = "Panama.Canal.MySQL.MySqlOptions";
        public int Port { get; set; } = 3306;
        public string TablePrefix { get; set; } = "Panama";
        public string Host { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Database { get; set; } = string.Empty;
        public int Heartbeat { get; set; } = 30;
        public int FailedRetries { get; set; } = 5;
        public bool StreamBinlog { get; set; } = true;
    }
}
