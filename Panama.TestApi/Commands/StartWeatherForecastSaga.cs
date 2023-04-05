using Panama.Canal.Channels;
using Panama.Canal.Extensions;
using Panama.Canal.Interfaces;
using Panama.Canal.Interfaces.Sagas;
using Panama.Extensions;
using Panama.Interfaces;
using Panama.TestApi;
using Panama.TestApi.Sagas;

namespace Panama.Tests.Commands
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
                    .Ack("foo.event.success")
                    .Nack("foo.event.failed")
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
