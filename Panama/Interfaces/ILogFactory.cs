namespace Panama.Interfaces
{
    public interface ILogFactory
    {
        ILog<T> CreateLogger<T>();
    }
}
