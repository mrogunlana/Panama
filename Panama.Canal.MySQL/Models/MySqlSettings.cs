using Panama.Interfaces;

namespace Panama.Canal.MySQL.Models
{
    public class MySqlSettings : IModel
    {
        public Version Version { set; get; } = new Version();
        public MySqlType Type { set; get; } = MySqlType.None;
        public string PublishedTable { set; get; } = string.Empty;
        public string ReceivedTable { set; get; } = string.Empty;
        public string SagaTable { set; get; } = string.Empty;
        public string OutboxTable { set; get; } = string.Empty;
        public string InboxTable { set; get; } = string.Empty;
        public string LockTable { set; get; } = string.Empty;
        public int PublishedTableId { get; set; } = 0;
        public int ReceivedTableId { get; set; } = 0;
        public int InboxTableId { get; set; } = 0;
        public int OutboxTableId { get; set; } = 0;
        public int SagaTableId { get; set; } = 0;
        public Dictionary<int, string> PublishedTableMap { set; get; } = new Dictionary<int, string>();
        public Dictionary<int, string> ReceivedTableMap { set; get; } = new Dictionary<int, string>();
        public Dictionary<int, string> OutboxTableMap { set; get; } = new Dictionary<int, string>();
        public Dictionary<int, string> InboxTableMap { set; get; } = new Dictionary<int, string>();
        public Dictionary<int, string> SagaTableMap { set; get; } = new Dictionary<int, string>();
    }
}
