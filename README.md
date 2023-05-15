# Panama - A fluent dotnet core library for event-based microservices

Panama is a dotnet core library based on the command query responsibility segregation design pattern [(CQRS overview)](https://learn.microsoft.com/en-us/azure/architecture/patterns/cqrs). 

The library is organized around `ICommand`, `IQuery`, `IValidate`, `IRollback` interfaces:

## ICommand
Commands are objects that can change the state of a domain. A chain of responsibilities in the form of multiple `ICommand` objects can be scaffolded on a handler to form a comprehensive domain event:

```
var result = await _provider
	.GetRequiredService<IHandler>()
	.Add(new User() { Email: diran.ogunlana@gmail.com })
	.Command<SaveUser>()
	.Command<PublishUser>()
	.Invoke();
```

These objects inherit from the `ICommand` interface and should only encapsulate a single business rule:
```
public class SaveUser : ICommand
{
	private UserDbContext _store;
	
	public SaveUser(UserDbContext store)
	{
		_store = store;
	}

	public bool Execute(IContext context)
	{
		var user = data.GetSingle<User>();
	
		_store.Entry(user).State = EntityState.Modified;
		_store.SaveChangesAsync();
	}
}
```

## IQuery
Query objects represent readonly data store operations such as retreiving a collection of entities by a condition from a data store:

```
public class GetUser : IQuery
{
	private UserDbContext _store;
	
	public GetUser(UserDbContext store)
	{
		_store = store;
	}

	public bool Execute(IContext context)
	{
		//get id from payload
		var id = context.KvpGetSingle<string, string>("User.Id");
		
		var user = context.Users
			.Where(s => s.Id == id)
			.FirstOrDefault();
		
		//save queried user to handler context for processing down stream
		context.Data.Add(user);
	}
}
```

## IValidate
Validators are objects that perform prerequisite operations against the handler context prior to query, command, or rollback operations: 
```
public class EmailNotNullOrEmpty : IValidation
{
	public bool Execute(IContext context)
	{
		var models = context.DataGet<User>();
		if (models == null)
			throw new ValidationException("User(s) cannot be found.");

		foreach (var user in models)
			if (string.IsNullOrEmpty(user.Email))
				throw new ValidationException($"{user.Name} email is not valid.");
	}
}
```

## IRollback
Rollback object perform the restoration operations against the domain in the event of an exception in processing the handler context. Take note of the use of the `Snapshot` filter on the context which creates a serialized copy of the object to preserve its original state:
```
public class RollbackUser : IRollback
{
	private UserDbContext _store;
	
	public RollbackUser(UserDbContext store)
	{
		_store = store;
	}

	public bool Execute(IContext context)
	{
		//get cached snapshot of previous state from handler context
		var existing = context.SnapshotGetSingle<User>();
		
		//save snapshot version e.g. prior to current changes
		_store.Entry(user).State = EntityState.Modified;
		
		await _store.SaveChangesAsync();
	}
}
```

Here is an example of a save domain event with validation and rollback capabilities: 
```
var result = await _provider
	.GetRequiredService<IHandler>()
	.Add(new User() { 
		Email = "diran.ogunlana@gmail.com", 
		FirstName = "Diran" })
	.Validate<UserEmailNotNullOrEmpty>()
	.Validate<UserFirstNameNotNullOrEmpty>()
	.Query<GetUserByEmail>()
	.Command<CreateUserSnapshot>()
	.Command<SaveUser>()
	.Command<PublishUser>()
	.Rollback<RollbackUser>()
	.Invoke();
```

# Panama Canal - A transactional event bus framework for distributed microservices

The Canal provides a transactional messaging framework with saga support for dotnet core. The Canal integrates with native dotnet core DI, logging [(dotnet core logging)](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/logging) and Entity Framework [(dotnet ef core)](https://learn.microsoft.com/en-us/ef/core/). Messages can be published using polling or event stream. Multiple message brokers can be configured and scoped for auto-scaling scenarios. 

## Panama Canal Services At A Glance: 

![image](https://user-images.githubusercontent.com/11683585/223599365-5b2a1d4f-a3cc-432b-b5d0-62c1188b4cb1.png)    

## Panama Canal Services At A Glance:

```
public class SaveWeatherForecast : ICommand
{
	private readonly IGenericChannelFactory _factory;
	private readonly WeatherForecastDbContext _store;

	public PublishWeatherForecast(
		IGenericChannelFactory factory, 
		WeatherForecastDbContext store)
	{
		_store = store;
		_factory = factory;
	}
	public async Task Execute(IContext context)
	{
		var model = context.Data.DataGet<WeatherForecast>();

		using (var channel = _factory.CreateChannel<DatabaseFacade, IDbContextTransaction>(_store.Database, context.Token))
		{
			_store.Entry(model).State = EntityState.Modified;
			_store.SaveChangesAsync();
			
			await context.Bus()
				.Channel(channel)
				.Token(context.Token)
				.Topic("forecast.created")
				.Data(model)
				.Post();

			await channel.Commit();
		}
	}
}
```

> In the above example, a `IChannel` which creates a transaction to emit the forecast message if and only if the forecast was stored successfully via the `Commit()` function. 

A saga can be used for more exhaustive use cases: 
```
var model = context.Data.DataGet<WeatherForecast>();

using (var channel = _factory.CreateChannel<DatabaseFacade, IDbContextTransaction>(_store.Database, context.Token))
{
	...
	
	await context.Saga<CreateWeatherForcastSaga>()
		.Channel(channel)
		.Data(model)
		.Start();

	await channel.Commit();
}
```
> see [CreateWeatherForcast](Samples/Panama.Samples.TestApi/Sagas/CreateWeatherForcast) for the full implementation sample

# Getting Started

Default Services Configuration:
> `UseDefaultStore` and `UseDefaultBroker` are in-memory services used in unit testing scenarios
```
services.AddPanama(
	configuration: builder.Configuration,
	setup: options => {
		options.UseCanal(canal => {
			canal.UseDefaultStore();
			canal.UseDefaultBroker();
			canal.UseDefaultScheduler();
		});
	});
```
> **NOTE:** Panama Canal must have a store and atleast one broker service configured

MySql & RabbitMQ Services Configuration:
```
services.AddPanama(
	configuration: builder.Configuration,
	setup: options => {
		options.UseCanal(canal => {
			canal.UseMySqlStore();
			canal.UseRabbitMq();
			canal.UseDefaultScheduler();
		});
	});
```

For native logging support, add the `Microsoft.Extensions.DependencyInjection` and `Microsoft.Extensions.Logging` nuget packages with the following configuration:
```
services.AddLogging(loggingBuilder => {
	
	loggingBuilder.ClearProviders();
	loggingBuilder.SetMinimumLevel(LogLevel.Trace);
	
	// configure Logging with NLog, ex:
	// loggingBuilder.AddNLog(_configuration);
});
```

`appsettings.json` or environment variables can be for initial options configurations:
```
{
  "ConnectionStrings": {
    "MySql": [DB_CONNECTION]
  },
  "Logging": {
    "LogLevel": {
      "Default": "Debug"
    }
  },
  "AllowedHosts": "*",
  "Panama": {
    "Canal": {
	  "Options": {
        "Scope": [CLUSTER_NAMESPACE]
      },
      "Brokers": {
        "RabbitMQ": {
          "Options": {
            "Port": [RABBIT_PORT],
            "Host": [RABBIT_HOST],
            "Username": [RABBIT_USERNAME],
            "Password": [RABBIT_PASSWORD]
          }
        }
      },
      "Stores": {
        "MySql": {
          "Options": {
            "Port": [DB_PORT],
            "Host": [DB_HOST],
            "Username": [DB_USERNAME],
            "Password": [DB_PASSWORD],
            "Database": [DB_NAME]
          }
        }
      }
    }
  }
}

```

For inquiries and support, contact: [Diran Ogunlana](mailto:diran.ogunlana@gmail.com)