namespace Panama.Canal.Interfaces
{
    public interface IGenericChannelFactory
    {
        IChannel<C, T> CreateChannel<C, T>(C client, CancellationToken token = default);
    }
}