namespace Panama.Canal.Interfaces
{
    public interface IBrokerFactory
    {
        IBrokerClient Create(string group);
    }
}