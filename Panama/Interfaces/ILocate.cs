namespace Panama.Core.Interfaces
{
    public interface ILocate
    {
        T Resolve<T>();
        T Resolve<T>(string name);
    }
}
