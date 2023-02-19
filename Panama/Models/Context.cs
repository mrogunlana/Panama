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
        public Context(
              IList<IModel> data
            , CancellationToken? token = null
            , Guid? handlerId = null
            , Guid? correlationId = null)
            : this()
        {
            Data = data;

            if (token.HasValue)
                Token = token.Value;
            if (handlerId.HasValue)
                HandlerId = handlerId.Value;
            if (correlationId.HasValue)
                CorrelationId = correlationId.Value;
        }
        public IList<IModel> Data { get; set; }
        public CancellationToken Token { get; set; }
        public Guid HandlerId { get; }
        public Guid CorrelationId { get; }
    }
}
