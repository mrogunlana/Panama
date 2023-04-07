using Panama.Canal.Channels;
using Panama.Canal.Extensions;
using Panama.Canal.Interfaces;
using Panama.Canal.Sagas.Extensions;
using Panama.Canal.Sagas.Stateless.Extensions;
using Panama.Canal.Sagas.Stateless.Interfaces;
using Panama.Extensions;
using Panama.Interfaces;
using Panama.TestApi.Sagas.CreateWeatherForcast;

namespace Panama.TestApi.Commands
{
    public class StartWeatherForecastSaga : ICommand
    {
        private readonly ISagaFactory _sagas;
        private readonly IDefaultChannelFactory _channels;

        public StartWeatherForecastSaga(
              IDefaultChannelFactory channels
            , ISagaFactory sagas)
        {
            _sagas = sagas;
            _channels = channels;
        }
        public async Task Execute(IContext context)
        {
            var models = context.Data.DataGet<WeatherForecast>();

            using (var channel = _channels.CreateChannel<DefaultChannel>())
            {
                await context.Bus()
                    .Channel(channel)
                    .Topic("foo.event")
                    .Group("foo")
                    .Data(models)
                    .Reply("foo.event.success")
                    .Post();

                await context.Saga<CreateWeatherForcastSaga>()
                    .Channel(channel)
                    .Data(models)
                    .Start();

                await channel.Commit();
            }
        }
    }
}
