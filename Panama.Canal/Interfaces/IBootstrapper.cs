namespace Panama.Canal.Interfaces
{
    public interface IBootstrapper 
    {
        bool Online { get; }
        Task On(CancellationToken cancellationToken);
        Task Off(CancellationToken cancellationToken);
    }
}
