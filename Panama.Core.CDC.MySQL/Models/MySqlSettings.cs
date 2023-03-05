﻿using Panama.Core.Interfaces;

namespace Panama.Core.CDC.MySQL
{
    public class MySqlSettings : IModel
    {
        public Version Version { set;  get; } = new Version();
        public MySqlType Type { set; get; } = MySqlType.None;
        public string PublishedTable { set; get; } = String.Empty;
        public string ReceivedTable { set; get; } = String.Empty;
        public string LockTable { set; get; } = String.Empty;
        public int PublishedTableId { get; set; } = 0;
        public int ReceivedTableId { get; set; } = 0;
        public Dictionary<int, string> PublishedTableMap { set; get; } = new Dictionary<int, string>();
        public Dictionary<int, string> ReceivedTableMap { set; get; } = new Dictionary<int, string>();
    }
}
