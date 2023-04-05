using Panama.Canal.Models;
using Panama.Interfaces;
using Stateless;

namespace Panama.Canal.Interfaces
{
    public interface ISaga
    {
        StateMachine<string, string>? StateMachine { get; }
        Task Configure();
        Task<IResult> Continue(SagaContext context);
    }
}