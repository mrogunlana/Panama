using Panama.Interfaces;

namespace Panama.Canal.Sagas.Stateless.Interfaces
{
    public interface ISagaStepEvent
    {
        Task<ISagaState> Execute(IContext context);
    }
}