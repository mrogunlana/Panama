﻿using Microsoft.Extensions.DependencyInjection;
using Panama.Canal.Models.Messaging;
using Panama.Canal.Sagas.Stateless.Interfaces;
using Panama.Interfaces;
using Stateless;

namespace Panama.Canal.Sagas.Stateless
{
    public class StatelessSagaTriggerFactory : ISagaTriggerFactory
    {
        private readonly IServiceProvider _provider;
        public StatelessSagaTriggerFactory(IServiceProvider provider)
        {
            _provider = provider;
        }
        public StateMachine<ISagaState, ISagaTrigger>.TriggerWithParameters<IContext> Create<T>(StateMachine<ISagaState, ISagaTrigger> machine) where T : ISagaTrigger
        {
            var result = _provider.GetRequiredService<T>();

            return machine.SetTriggerParameters<IContext>(result);
        }

        public StateMachine<ISagaState, ISagaTrigger>.TriggerWithParameters<IContext> Create(string type, StateMachine<ISagaState, ISagaTrigger> machine)
        {
            var result = Type.GetType(type);
            if (result == null)
                throw new InvalidOperationException($"Header: {Headers.SagaTrigger} type cannot be found.");

            var trigger = (ISagaTrigger)_provider.GetRequiredService(result);
            var parameter = machine.SetTriggerParameters<IContext>(trigger);

            return parameter;
        }
    }
}