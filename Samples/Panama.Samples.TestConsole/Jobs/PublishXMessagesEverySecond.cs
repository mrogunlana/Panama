﻿using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Panama.Samples.TestConsole.Models;
using Quartz;
using System.Text;

namespace Panama.Samples.TestConsole.Jobs
{
    [DisallowConcurrentExecution]
    public class PublishXMessagesEverySecond : IJob
    {
        private readonly ILogger<PublishXMessagesEverySecond> _log;
        private readonly IServiceProvider _provider;

        public PublishXMessagesEverySecond(
              IServiceProvider provider,
              ILogger<PublishXMessagesEverySecond> log)
        {
            _log = log;
            _provider = provider;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            try
            {
                /*
             *  1. get api host from env vars
             *  2. create test forcast
             *  3. loop 10 rest calls to forecast service
             * 
             */
                var configuration = _provider.GetRequiredService<IConfiguration>();
                var host = Environment.GetEnvironmentVariable("SERVICE_HOST") ?? configuration.GetValue<string>("HOST");
                var interval = Convert.ToInt32(Environment.GetEnvironmentVariable("POST_COUNT") ?? configuration.GetValue<string>("POST_COUNT") ?? "10");
                var url = $"http://{host}/WeatherForecast/";

                for (int i = 0; i < interval; i++)
                {
                    using (var client = new HttpClient())
                    {
                        var forecast = new WeatherForecast();

                        // randomly generate forecast
                        forecast.TemperatureC = new Random().Next(10, 100);
                        forecast.Summary = $"Generated by {typeof(PublishXMessagesEverySecond)}";
                        forecast.Date = DateTime.Now.AddDays(-60).AddDays(new Random().Next((DateTime.Today - DateTime.Now.AddDays(-60)).Days));

                        // serialize content
                        var content = new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(forecast), Encoding.UTF8, "application/json");

                        client.DefaultRequestHeaders.Add("User-Agent", "Panama.Samples.TestConsole");

                        _log.LogDebug($"Publishing forecast to: {url}.");

                        await client.PostAsync(url, content);
                    }
                }
            }
            catch (Exception ex)
            {
                _log.LogError(ex, ex.Message);
            }
        }
    }
}