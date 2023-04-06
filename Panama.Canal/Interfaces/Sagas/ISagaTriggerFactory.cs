using Panama.Interfaces;
using Stateless;

namespace Panama.Canal.Interfaces.Sagas
{
    public interface ISagaTriggerFactory
    {
        StateMachine<ISagaState, ISagaTrigger>.TriggerWithParameters<IContext> Create<T>(StateMachine<ISagaState, ISagaTrigger> machine) where T : ISagaTrigger;
        StateMachine<ISagaState, ISagaTrigger>.TriggerWithParameters<IContext> Create(string type, StateMachine<ISagaState, ISagaTrigger> machine);
    }
}