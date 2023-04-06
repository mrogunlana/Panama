using Panama.Canal.Interfaces.Sagas;
using Panama.Canal.Models;
using Panama.Interfaces;
using Stateless;

namespace Panama.Canal.Interfaces.Sagas
{
    public interface ISaga
    {
        string ReplyTopic { get; }
        List<ISagaState> States { get; set; }
        List<StateMachine<ISagaState, ISagaTrigger>.TriggerWithParameters<IContext>> Triggers { get; set; }
        StateMachine<ISagaState, ISagaTrigger> StateMachine { get; }
        void Configure(IContext context);
        Task Start(IContext context);
        Task<IResult> Continue(SagaContext context);
    }
}