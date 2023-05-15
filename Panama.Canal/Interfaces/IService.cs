namespace Panama.Canal.Interfaces
{
    public interface ICanalService 
    {
        bool Online { get; }
        Task On(CancellationToken cancellationToken);
        Task Off(CancellationToken cancellationToken);
    }
}
