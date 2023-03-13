using Panama.Interfaces;

namespace Panama.Canal.MySQL
{
    public class MySqlOptions : IModel
    {
        public int Port { get; set; } = 3306;
        public string TablePrefix { get; set; } = "Panama";
        public string Host { get; set; } = String.Empty;
        public string Username { get; set; } = String.Empty;
        public string Password { get; set; } = String.Empty;
        public string Database { get; set; } = String.Empty;
        public int Heartbeat { get; set; } = 30;
        public int FailedRetries { get; set; } = 5;
        public int Version { get; set; } = 1;
    }
}
