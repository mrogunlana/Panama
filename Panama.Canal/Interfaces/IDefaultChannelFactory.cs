namespace Panama.Canal.Interfaces
{
    public interface IDefaultChannelFactory
    {
        T CreateChannel<T>(CancellationToken token = default) where T : IChannel;
    }
}