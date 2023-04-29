namespace Panama.Interfaces
{
    public interface IContext
    {
        string Id { get; set; }
        string CorrelationId { get; set; }
        IList<IModel> Data { get; }
        CancellationToken Token { get; set; }
        IServiceProvider Provider { get; }
        object? Transaction { get; set; }
    }
}
