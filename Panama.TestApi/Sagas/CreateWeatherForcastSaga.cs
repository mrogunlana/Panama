using Panama.Canal.Interfaces;
using Panama.Interfaces;

namespace Panama.TestApi.Sagas
{
    public class CreateWeatherForcastSaga : ISaga
    {
        public Task<Interfaces.IResult> Start(IContext context)
        {
            throw new NotImplementedException();
        }
        public Task<Interfaces.IResult> Continue(IContext context)
        {
            throw new NotImplementedException();
        }
    }
}
