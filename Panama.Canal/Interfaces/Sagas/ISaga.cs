using Panama.Canal.Interfaces.Sagas;
using Panama.Canal.Models;
using Panama.Interfaces;
using Stateless;

namespace Panama.Canal.Interfaces.Sagas
{
    public interface ISaga
    {
        List<ISagaState> States { get; set; }
        List<ISagaTrigger> Triggers { get; set; }
        StateMachine<ISagaState, ISagaTrigger> StateMachine { get; }
        Task Configure(IContext context);
        Task<IResult> Start();
        Task<IResult> Continue(SagaContext context);
    }
}