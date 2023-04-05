using Panama.Canal.Interfaces;
using Panama.Canal.Models;
using Stateless;

namespace Panama.TestApi.Sagas
{
    public class CreateWeatherForcastSaga : ISaga
    {
        public StateMachine<string, string>? StateMachine { get; }

        public Task Configure()
        {
            throw new NotImplementedException();
        }

        public Task<Interfaces.IResult> Continue(SagaContext context)
        {
            throw new NotImplementedException();
        }
    }
}
