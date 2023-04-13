namespace Panama.Canal.Brokers.Interfaces
{
    public interface IBrokerProcess
    {
        bool IsHealthy();

        void Restart(bool force = false);
    }
}