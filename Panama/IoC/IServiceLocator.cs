namespace Panama.Core.IoC
{
    public interface IServiceLocator
    {
        T Resolve<T>();
        T Resolve<T>(string name);
    }
}
