﻿using Panama.Core.Interfaces;

namespace Panama.Core.CDC.MySQL
{
    public class Outbox : IModel
    {
        public int _Id { get; set; }
        public string Id { get; set; } = String.Empty;
        public string CorrelationId { get; set; } = String.Empty;
        public string Version { get; set; } = String.Empty;
        public string Name { get; set; } = String.Empty;
        public string Group { get; set; } = String.Empty;
        public string Content { get; set; } = String.Empty;
        public int Retries { get; set; }
        public DateTime Created { get; set; }
        public DateTime Expires { get; set; }
        public string Status { get; set; } = String.Empty;
    }
}
