using Microsoft.Extensions.DependencyInjection;
using Panama.Canal.Interfaces;
using Panama.Canal.Models;
using Panama.Canal.Models.Filters;
using Panama.Extensions;
using Panama.Interfaces;
using System.Collections.Concurrent;
using System.Linq;

namespace Panama.Canal.Extensions
{
    public static class ConcurrentQueueExtensions
    {
        public static void EnqueueResult<T>(this ConcurrentQueue<T> queue, IResult result)
            where T : IModel
        {
            if (result == null)
                return;
            if (!result.Success)
                return;

            foreach (var data in result.Data.QueueGet<T>())
                queue.Enqueue(data);
        }
    }
}
