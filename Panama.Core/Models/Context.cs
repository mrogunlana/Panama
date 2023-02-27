using Panama.Core.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Panama.Core.Models
{
    public class Context : IContext
    {
        public Context()
        {
            if (Data == null)
                Data = new List<IModel>();
        }

        public Context(CancellationToken? token = null)
            : this()
        {
            if (token.HasValue)
                Token = token.Value;
        }
        public Context(
            CancellationToken? token = null
            , string id = null
            , string correlationId = null)
            : this()
        {
            if (token.HasValue)
                Token = token.Value;
            if (!string.IsNullOrEmpty(id))
                Id = id;
            if (!string.IsNullOrEmpty(correlationId))
                CorrelationId = correlationId;
        }

        public Context(IEnumerable<IModel> data
            , CancellationToken? token = null
            , string id = null
            , string correlationId = null)
            : this(token, id, correlationId)
        {
            Data = data.ToList();
        }

        public Context(
              IModel data
            , CancellationToken? token = null
            , string id = null
            , string correlationId = null)
            : this(token, id, correlationId)
        {
            Data.Add(data);
        }

        public IList<IModel> Data { get; set; }
        public CancellationToken Token { get; set; }
        public string Id { get; set; }
        public string CorrelationId { get; set; }
        public ILocate Locator { get; }
    }
}
