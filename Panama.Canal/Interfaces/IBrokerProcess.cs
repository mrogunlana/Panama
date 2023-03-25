namespace Panama.Canal.Interfaces
{
    public interface IBrokerProcess
    {
        bool IsHealthy();

        void Restart(bool force = false);
    }
}