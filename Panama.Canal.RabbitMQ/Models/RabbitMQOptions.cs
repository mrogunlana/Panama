using Panama.Canal.Interfaces;
using Panama.Interfaces;

namespace Panama.Canal.RabbitMQ.Models
{
    public class RabbitMQOptions : IModel 
    {
        public static string Section { get; set; } = "Panama.Canal.RabbitMQ.RabbitMQOptions";
        public string UserName { get; set; } = "guest";
        public string Password { get; set; } = "guest";
        public string HostName { get; set; } = "localhost";
        public string HostAddress { get; set; } 
        public int Port { get; set; } = 5672;
        public string VHost { get; set; } = "/";
        public string Exchange { get; set; } = "panama.default.router";
        public bool PublishAcks { get; set; } = true;
        public int MessageTTL { get; set; } = 864000000; //in ms
        public string QueueMode { get; set; } = default!;
        public RabbitMQOptions()
        {
            HostAddress = $"{HostName}:{Port}";
        }
    }
}
