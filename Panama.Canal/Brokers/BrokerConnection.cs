using Panama.Interfaces;

namespace Panama.Canal.Brokers
{
    public class BrokerConnection : IDisposable, IModel
    {
        public bool IsOpen { get; set; } = false;

        public BrokerConnection()
        {
            IsOpen = true;
        }
        public void Dispose()
        {
            // ignore
        }
    }
}
