using Panama.Interfaces;

namespace Panama.Canal.Sagas.Stateless.Interfaces
{
    public interface ISagaEvent
    {
        Task<ISagaState> Execute(IContext context);
    }
}