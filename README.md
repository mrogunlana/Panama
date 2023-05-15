# Panama - An elegant and concise dotnet core library that provides a  fluent API for event-based domain transactions  

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
	.Validate<EmailNotNullOrEmpty>()
	.Query<GetUserByEmail>()
	.Command<CreateUserSnapshot>()
	.Command<SaveUser>()
	.Command<PublishUser>()
	.Rollback<RollbackUser>()
	.Invoke();
```

# Panama Canal - A dotnet core framework built to develop, test, and scale distributed microservices

