using Panama.Canal.Models;
using Panama.Interfaces;

namespace Panama.Canal.Interfaces
{
    public interface ISaga
    {
        Task<IResult> Start(SagaContext context);
        Task<IResult> Continue(SagaContext context);
    }
}