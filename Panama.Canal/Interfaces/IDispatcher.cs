﻿using Panama.Canal.Models.Messaging;
using Panama.Interfaces;

namespace Panama.Canal.Interfaces
{
    public interface IDispatcher : ICanalService
    {
        IInvoke Brokers { get; set; }
        IInvoke Subscriptions { get; set; }

        ValueTask Publish(InternalMessage message, CancellationToken? token = null);

        ValueTask Execute(InternalMessage message, CancellationToken? token = null);

        ValueTask Schedule(InternalMessage message, DateTime delay, object? transaction = null, CancellationToken? token = null);
    }
}
