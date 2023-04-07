using Panama.Canal.Sagas.Models;
using Panama.Interfaces;

namespace Panama.Canal.Sagas.Interfaces
{
    public interface ISaga
    {
        Task Start(IContext context);
        Task<IResult> Continue(SagaContext context);
    }
}