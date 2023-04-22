namespace Panama.Canal.Interfaces
{
    public interface IMarina
    {
        IBus GetBus(CancellationToken? token = null);
    }
}
