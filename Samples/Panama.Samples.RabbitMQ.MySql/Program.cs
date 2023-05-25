using Microsoft.EntityFrameworkCore;
using MySqlConnector.Logging;
using NLog.Extensions.Logging;
using Panama;
using Panama.Canal;
using Panama.Canal.MySQL;
using Panama.Canal.RabbitMQ;
using Panama.Samples.RabbitMQ.MySql.Models;
using Panama.Samples.RabbitMQ.MySQL.Contexts;
using System.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Add logging configurations
NLog.Extensions.Logging.ConfigSettingLayoutRenderer.DefaultConfiguration = builder.Configuration;
MySqlConnectorLogManager.Provider = new MySqlConnector.Logging.NLogLoggerProvider();

// Add services to the container.
builder.Services.AddControllers().AddNewtonsoftJson();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add Panama Canal Services: MySql, RabbitMq, Default Quartz Scheduler
builder.Services.AddPanama(
    configuration: builder.Configuration,
    setup: options => {
        options.UseCanal(canal => {
            canal.UseMySqlStore();
            canal.UseRabbitMq();
            canal.UseDefaultScheduler();
        });
    });

builder.Services.AddDbContext<AppDbContext>(options => {
    var connectionString = builder.Configuration.GetConnectionString("MySql");
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString))
        .LogTo(Console.WriteLine, LogLevel.Information)
        .EnableDetailedErrors();
});

builder.Services.AddLogging(loggingBuilder => {
    // configure Logging with NLog
    loggingBuilder.ClearProviders();
    loggingBuilder.SetMinimumLevel(LogLevel.Trace);
    loggingBuilder.AddNLog(builder.Configuration);
});

// For testing purposes -- state object
builder.Services.AddSingleton<State>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
