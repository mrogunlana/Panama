namespace Panama.Canal.Interfaces
{
    public interface IInitialize
    {
        Task Invoke(CancellationToken token);
    }
}
