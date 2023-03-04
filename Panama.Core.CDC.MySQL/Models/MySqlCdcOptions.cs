using Panama.Core.Interfaces;

namespace Panama.Core.CDC.MySQL
{
    public class MySqlCdcOptions : IModel
    {
        public int Port { get; set; } = 3306;
        public int PublishedTableId { get; set; } = 0;
        public int ReceivedTableId { get; set; } = 0;
        public string Host { get; set; } = String.Empty;
        public string Username { get; set; } = String.Empty;
        public string Password { get; set; } = String.Empty;
        public string Database { get; set; } = String.Empty;
        public int Heartbeat { get; set; } = 30;
        public int Version { get; set; } = 1;
    }
}
