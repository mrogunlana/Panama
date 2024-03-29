﻿using Panama.Canal.Models.Messaging;
using Panama.Interfaces;
using System.Collections.Concurrent;

namespace Panama.Canal.Interfaces
{
    public interface IChannel<C, T> : IChannel
    {
        void Open(C channel, CancellationToken token = default);
    }

    public interface IChannel : IDisposable
    {
        object? Current { get; set; }
        ConcurrentQueue<InternalMessage> Queue { get; }
        IInvoke Invoker { get; set; }

        void Rollback(CancellationToken token = default);
        Task Commit(CancellationToken token = default);
        Task Flush(CancellationToken token = default);
    }
}