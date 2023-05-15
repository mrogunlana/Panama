using Panama.Canal.Brokers.Interfaces;

namespace Panama.Canal.RabbitMQ.Models
{
    public class RabbitMQOptions : IBrokerOptions
    {
        public string Username { get; set; } = "guest";
        public string Password { get; set; } = "guest";
        public string Host { get; set; } = "localhost";
        public string HostAddress { get; set; } 
        public int Port { get; set; } = 5672;
        public string VHost { get; set; } = "/";
        public string Exchange { get; set; } = "panama.default.router";
        public bool PublishAcks { get; set; } = true;
        public int MessageTTL { get; set; } = 864000000; //in ms
        public string QueueMode { get; set; } = default!;
        public ushort QosPrefetchCount { get; private set; } = 0;
        public bool QosGlobal { get; private set; } = false;
        public bool Default { get; set; } = false;

        public RabbitMQOptions()
        {
            HostAddress = $"{Host}:{Port}";
        }
    }
}
