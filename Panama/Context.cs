using Panama.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading;

namespace Panama.Core
{
    public class Context : IContext
    {
        public Context()
        {
            if (Data == null)
                Data = new List<IModel>();
        }
        public Context(IList<IModel> data, CancellationToken token)
            : this()
        {
            Data = data;
            Token = token;
        }
        public IList<IModel> Data { get; set; }
        public CancellationToken Token { get; set; }
    }
}
