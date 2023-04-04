using Panama.Canal.Interfaces;
using Panama.Canal.Models;
using Panama.Interfaces;

namespace Panama.TestApi.Sagas
{
    public class CreateWeatherForcastSaga : ISaga
    {
        public Task<Interfaces.IResult> Start(SagaContext context)
        {
            throw new NotImplementedException();
        }
        public Task<Interfaces.IResult> Continue(SagaContext context)
        {
            throw new NotImplementedException();
        }
    }
}
