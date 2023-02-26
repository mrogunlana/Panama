using Panama.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Panama.Core.Models
{
    public class Context : IContext
    {
        public Context(ILocate locator)
        {
            if (Data == null)
                Data = new List<IModel>();

            Locator = locator;
        }
        
        public Context(
              IList<IModel> data
            , ILocate locator = null
            , CancellationToken? token = null
            , Guid? handlerId = null
            , Guid? correlationId = null)
            : this(locator)
        {
            Data = data;

            if (token.HasValue)
                Token = token.Value;
            if (handlerId.HasValue)
                Id = handlerId.Value;
            if (correlationId.HasValue)
                CorrelationId = correlationId.Value;
        }
        public IList<IModel> Data { get; set; }
        public CancellationToken Token { get; set; }
        public Guid Id { get; }
        public Guid CorrelationId { get; }
        public ILocate Locator { get; }
    }
}
