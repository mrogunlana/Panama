using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Panama.Canal.Sagas.Stateless.Extensions;
using Panama.Canal.Sagas.Stateless.Interfaces;
using Panama.Canal.Sagas.Stateless.Models;
using Panama.Canal.Tests.Models;
using Panama.Canal.Tests.Sagas.CreateFoo.States;
using Panama.Canal.Tests.Sagas.CreateFoo.Triggers;
using Panama.Extensions;
using Panama.Interfaces;
using Panama.Models;
using Stateless;

namespace Panama.Canal.Tests
{
    [TestClass]
    public class StatelessTest
    {
        private IServiceProvider _provider;
        
        public StatelessTest()
        {
            var services = new ServiceCollection();

            services.AddOptions();
            services.AddLogging();
            services.AddSingleton<IServiceCollection>(_ => services);

            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.test.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

            services.AddSingleton(configuration);
            services.AddSingleton<IConfiguration>(configuration);
            services.AddSingleton<Models.State>();

            services.AddPanama(
                configuration: configuration,
                setup: options => {
                    options.UseCanal(canal => {
                        canal.UseDefaultStore();
                        canal.UseDefaultBroker();
                        canal.UseDefaultScheduler();
                    });
                });

            _provider = services.BuildServiceProvider();
        }

        [TestMethod]
        public void VerifySimpleSagaStateMachine()
        {
            var _states = _provider.GetRequiredService<ISagaStateFactory>();
            var _triggers = _provider.GetRequiredService<ISagaTriggerFactory>();

            var states = new List<ISagaState>() { _states.Create<NotStarted>() };
            var triggers = new List<StateMachine<ISagaState, ISagaTrigger>.TriggerWithParameters<IContext>>();
            
            var machine = new StateMachine<ISagaState, ISagaTrigger>(states.First());

            states.Add(_states.Create<CreateFooRequested>());
            states.Add(_states.Create<CreateFooRequestAnswered>());
            states.Add(_states.Create<CreateFooCreated>());
            states.Add(_states.Create<CreateFooFailed>());
            states.Add(_states.Create<CreateFooRollbackRequested>());
            states.Add(_states.Create<CreateFooRollbackAnswered>());
            states.Add(_states.Create<CreateFooComplete>());

            triggers.Add(_triggers.Create<CreateNewFoo>(machine));
            triggers.Add(_triggers.Create<ReviewCreateFooAnswer>(machine));
            triggers.Add(_triggers.Create<RollbackCreateFoo>(machine));
            triggers.Add(_triggers.Create<CompleteNewFoo>(machine));

            machine.Configure(states.Get<NotStarted>())
                .PermitDynamic(triggers.Get<CreateNewFoo>(), (context) => {
                    return states.Get<CreateFooRequestAnswered>();
                });

            machine.Configure(states.Get<CreateFooRequestAnswered>())
                .PermitDynamic(triggers.Get<ReviewCreateFooAnswer>(), (context) => {
                    return states.Get<CreateFooCreated>();
                })
                .OnExit((e) =>
                {
                    //decide here to fire next trigger based 
                    //on result:

                    if (e.Destination == states.Get<CreateFooCreated>())
                        machine.Fire(triggers.Get<CompleteNewFoo>(), e?.Parameters[0]);
                    else if (e.Destination == states.Get<CreateFooFailed>())
                        machine.Fire(triggers.Get<RollbackCreateFoo>(), e?.Parameters[0]);
                    else
                        throw new InvalidOperationException($"Unhandled state transition for: {e.Destination} ");
                });

            machine.Configure(states.Get<CreateFooFailed>())
                .PermitDynamic(triggers.Get<RollbackCreateFoo>(), (context) => {
                    return states.Get<CreateFooFailed>();
                });

            machine.Configure(states.Get<CreateFooCreated>())
                .PermitDynamic(triggers.Get<CompleteNewFoo>(), (context) => {
                    return states.Get<CreateFooComplete>();
                });

            var context = new Context(_provider)
                .Add(new Foo());

            machine.Fire(triggers.Get<CreateNewFoo>(), context);
            machine.Fire(triggers.Get<ReviewCreateFooAnswer>(), context);

            Assert.IsTrue(machine.State == states.Get<CreateFooComplete>());
        }
    }
}