using Panama.Core.Interfaces;

namespace Panama.Core.CDC.Models
{
    public class BrokerOptions : IModel
    {
        public string Name { get; set; } = string.Empty;
        public string Endpoint { get; set; } = string.Empty;

        public BrokerOptions(string name, string endpoint)
        {
            Name = name;    
            Endpoint = endpoint;
        }
    }
}
