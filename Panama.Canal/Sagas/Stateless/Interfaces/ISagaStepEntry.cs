using Panama.Interfaces;

namespace Panama.Canal.Sagas.Stateless.Interfaces
{
    public interface ISagaStepEntry
    {
        Task<IResult> Execute(IContext context);
    }
}