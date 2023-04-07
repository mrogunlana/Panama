using Panama.Canal.Sagas.Interfaces;
using Panama.Interfaces;
using Stateless;

namespace Panama.Canal.Sagas.Stateless.Interfaces
{
    public interface IStatelessSaga : ISaga
    {
        string ReplyTopic { get; }
        List<ISagaState> States { get; set; }
        List<StateMachine<ISagaState, ISagaTrigger>.TriggerWithParameters<IContext>> Triggers { get; set; }
        StateMachine<ISagaState, ISagaTrigger> StateMachine { get; }
        void Configure(IContext context);
    }
}