using Panama.Interfaces;

namespace Panama.Canal.Sagas.Stateless.Interfaces
{
    public interface ISagaStepExit
    {
        Task<IResult> Execute(IContext context);
    }
}