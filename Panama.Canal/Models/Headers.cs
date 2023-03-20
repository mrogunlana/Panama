namespace Panama.Canal.Models
{
    public static class Headers 
    {
        public const string CorrelationId = "panama-correlation-id";
        public const string Id = "panama-message-id";
        public const string Name = "panama-message-name";
        public const string Broker = "panama-message-broker";
        public const string Group = "panama-message-group";
        public const string Type = "panama-message-type";
        public const string Ack = "panama-callback-ack";
        public const string Nack = "panama-callback-nack";
        public const string Exception = "panama-exception";
        public const string Created = "panama-created-time";
        public const string Sent = "panama-sent-time";
        public const string Delay = "panama-delay-time";
    }
}
