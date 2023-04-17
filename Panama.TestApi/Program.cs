using Panama;
using Panama.Canal;
using Panama.Canal.Extensions;
using Panama.Canal.MySQL;
using System.Reflection;

var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddJsonFile($"appsettings.{env}.json", true, true);

builder.Services.AddControllers().AddNewtonsoftJson();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var assemblies = new List<Assembly>();

assemblies.Add(Assembly.GetExecutingAssembly());
assemblies.AddRange(AppDomain.CurrentDomain.GetAssemblies());
assemblies.AddRange(Assembly
    .GetExecutingAssembly()
    .GetReferencedAssemblies()
    .Select(x => Assembly.Load(x))
    .ToList());

var domain = assemblies.ToArray();

builder.Services.AddPanama(
    assemblies: domain,
    configuration: builder.Configuration,
    setup: options => {
        options.UseCanal();
        options.UseMysql();
        options.UseDefaultBroker();
    });

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
