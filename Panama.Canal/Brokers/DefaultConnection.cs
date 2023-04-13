using Panama.Interfaces;

namespace Panama.Canal.Brokers
{
    public class DefaultConnection : IDisposable, IModel
    {
        public bool IsOpen { get; set; } = false;

        public DefaultConnection()
        {
            IsOpen = true;
        }
        public void Dispose()
        {
            // ignore
        }
    }
}
