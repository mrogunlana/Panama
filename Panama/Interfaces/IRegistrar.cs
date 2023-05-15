using Microsoft.Extensions.DependencyInjection;

namespace Panama.Interfaces
{
    public interface IRegistrar
    {
        Type Marker { get; }
        void AddServices(IServiceCollection services);
        void AddAssemblies(IServiceCollection services);
        void AddConfigurations(IServiceCollection services);
    }
}