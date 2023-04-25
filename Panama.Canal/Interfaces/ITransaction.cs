﻿using Microsoft.Extensions.Hosting;
using Panama.Canal.Models.Messaging;
using Quartz;

namespace Panama.Canal.Interfaces
{
    public interface ITransaction 
    {
        bool AutoCommit { get; set; }
        object? DbTransaction { get; set; }

        void Queue(InternalMessage message);
        
        Task Commit(CancellationToken token = default);
        Task Rollback(CancellationToken token = default);
        Task Flush(CancellationToken token = default);
    }
}
