﻿using Panama.Canal.Models;

namespace Panama.Canal.Interfaces
{
    public interface IBrokerClient : IDisposable
    {
        ICollection<string> GetOrAddTopics(IEnumerable<string> topics) => topics.ToList();

        void Subscribe(IEnumerable<string> topics);
        void Listen(TimeSpan timeout, CancellationToken cancellationToken);
        void Commit(object? sender);
        void Reject(object? sender);

        public Func<TransientMessage, object?, Task>? OnCallback { get; set; }
    }
}