namespace Panama.Canal.Models
{
    public static class Headers 
    {
        public const string CorrelationId = "panama-correlation-id";
        public const string Id = "panama-message-id";
        public const string Name = "panama-message-name";
        public const string Broker = "panama-message-broker";
        public const string Instance = "panama-message-broker-instance";
        public const string Group = "panama-message-group";
        public const string Type = "panama-message-type";
        public const string Reply = "panama-message-reply";
        public const string Exception = "panama-exception";
        public const string Created = "panama-created-time";
        public const string Sent = "panama-sent-time";
        public const string Delay = "panama-delay-time";
        public const string StoreStrategy = "panama-store-strategy";
        public const string SagaType = "panama-saga-type";
        public const string SagaId = "panama-saga-type";
        public const string SagaTrigger = "panama-saga-trigger";
    }
}
