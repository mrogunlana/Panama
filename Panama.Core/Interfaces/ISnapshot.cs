namespace Panama.Core.Interfaces
{
    public interface ISnapshot<T> : IModel
        where T : IModel 
    { 
        T Value { get; }
    }
}
