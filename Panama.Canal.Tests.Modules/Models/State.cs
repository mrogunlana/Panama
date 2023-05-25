using Panama.Interfaces;
using System.Collections.Concurrent;

namespace Panama.Canal.Tests.Modules.Models
{
    public class State : IModel
    {
        public ConcurrentBag<IModel> Data { get; set; }
        public State()
        {
            if (Data == null)
                Data = new ConcurrentBag<IModel>();
        }

        public void Reset() => Data.Clear();
    }
}
