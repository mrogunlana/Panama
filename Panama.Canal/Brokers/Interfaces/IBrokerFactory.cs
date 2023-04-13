namespace Panama.Canal.Brokers.Interfaces
{
    public interface IBrokerFactory
    {
        IBrokerClient Create(string group);
    }
}