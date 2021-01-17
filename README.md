# Panama.Core - A .NET Core 3 port of Panama.NET

Panama is a unique design pattern, created to simplify software design and more suitable in designing systems for over 70% of medium and large scale business design projects. It's goal is to reduce complexity normally found in N-Tier application where there are multiple layers (i.e. Service Layer, DataAcces Layer) required to invoke commands (i.e. save an object to a database). With Panama, the ability to invoke commands is reduced to one object that can be called directly from the UI or other top layer that a user or service may interact with directly.

## Getting Started
Panama is built around a central Handler class that uses a fluent api to wire up validators and commands and invoked using the Invoke() method. 

![alt text](https://raw.githubusercontent.com/mrogunlana/Panama/master/screenshots/The-Command-Handler-Architecture-by-Diran-Ogunlana-012.jpg "The-Command-Handler-Architecture-by-Diran-Ogunlana-012.jpg")

### The Handler class and IoC
The Handler class constructor takes in a ServiceLocator that is used to find commands and validators. The project includes a sample for use with Autofac, but any IoC container could work, it would just need to be implemented behind an IServiceLocator interface. Commands implement the ICommand interface and validators implement IValidator interface. Depending on how your IoC of choice works, you would wire up your specific implementations to the interface. Or if your IoC supports auto locating classes by interface, all the classes can be wired up and instantiated. Here is a sample of how to use Autofac to locate and instantiate all classes implementing the IValidator interface as singletons.

```c#
 var builder = new ContainerBuilder();
 
//Register all validators -- singletons
builder.RegisterAssemblyTypes(AppDomain.CurrentDomain.GetAssemblies())
       .Where(t => t.IsAssignableTo<IValidation>())
       .Named<IValidation>(t => t.Name)
       .AsImplementedInterfaces()
       .SingleInstance();
```
### Commands

Commands must implement the ICommand interface found in the dev.Core.Commands namespace
The ICommand interface currently specifies just one method:
```c#
    void Execute(List<IModel> data);
```
The interface allows Command classes to act on IModel objects (IModel is explained in further section below). Sample commands are provide in the project in the dev.Business.Commands namespace. Here is the SaveUser command implementation. Commands are only run after all Validators are run.
```
 public class SaveUser : ICommand
    {
        private IQuery _query;
        public SaveUser(IQuery query)
        {
            _query = query;
        }
        public void Execute(List<IModel> data)
        {
            var users = data.Get<User>();

            foreach (var user in users)
                _query.Save(user, new { user.ID });
        }
    }
```

### Validators

Validators must implement the IValidator interface which currently specifies two methods:
```
        bool IsValid(List<IModel> data);
        string Message();
```
The interface allows Validator classes to act on IModel objects (IModel is explained in further section below). Sample validators are provided in the project in the dev.Business.Validators namespace. Here is the EmailNotNullOrEmpty validator implementation. 
```
public class EmailNotNullOrEmpty : IValidation
    {
        public bool IsValid(List<IModel> data)
        {
            var models = data.Get<User>();
            if (models == null)
                return false;

            foreach (var user in models)
                if (string.IsNullOrEmpty(user.Email))
                    return false;

            return true;
        }

        public string Message() => "Email is required.";
    }
```


All Validators that are set on the Handler must return true for the IsValid method before the Commands that are set on the Handler are able torun.

### Models

Commands and Validators act on Models, which are classes that implement the IModel interface to represent model classes that are persisted to a database, The IModel interface specifies one property that represents the primary key of this row in it's respective table:
```
public interface IModel
{
    int _ID { get; set; }
}
```
The model framework is found in the dev.Entities project. This project implements the Dapper ORM framework for performing database actions as well as mapping table data onto objects. The project includes a sample model class called User:
```
public class User : IModel
{
        public User()
        {
            if (Modified == DateTime.MinValue)
                Modified = DateTime.Now;
        }
        public int _ID { get; set; }
        public Guid ID { get; set; }
        public Guid UserRoleId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string ConfirmPassword { get; set; }
        public bool Enabled { get; set; }
        public bool KeepAlive { get; set; }
        public DateTime Created { get; set; }
        public DateTime Modified { get; set; }
}
```

Also included is the UserMap class which uses DapperExtensions to map between table data and the User model class.

```
public class UserMap : ClassMapper<User>
{
        public UserMap()
        {
            Table("User");

            Map(x => x._ID).Key(KeyType.Identity);
            Map(x => x.ConfirmPassword).Ignore();
            Map(x => x.Created).ReadOnly();

            AutoMap();
        }
}
```

### Setting up the Sample projects
Two sample projects are included to serve as examples for configuring a Web Api application and a console application. There is a also a sql script called create.sql under the dev.Sql folder which will create the tables necessary to run the samples. These will need to be added to an existing database and configured in the config files in both projects. 


### More Info

For more detailed info on this project please see the following:
* [Intro to Command-Handler-Pattern](https://youtu.be/T0Nku5qsEqg) - video introduction
* [Command-Handler-Pattern Manual](https://goo.gl/6KAr37) - pdf
