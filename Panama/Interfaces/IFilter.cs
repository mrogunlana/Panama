namespace Panama.Interfaces
{
    public interface IFilter<T> : IModel
        where T : IModel 
    { 
        T Value { get; }
    }
}
