using Panama.Core.Interfaces;
using System.Collections.Generic;
using System.Linq;
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
              ILocate locator
            , CancellationToken? token = null)
            : this(locator)
        {
            if (token.HasValue)
                Token = token.Value;
        }

        public Context(
              IEnumerable<IModel> data
            , ILocate locator = null
            , CancellationToken? token = null
            , string id = null
            , string correlationId = null)
            : this(locator)
        {
            Data = Data.ToList();

            if (token.HasValue)
                Token = token.Value;
            if (!string.IsNullOrEmpty(id))
                Id = id;
            if (!string.IsNullOrEmpty(correlationId))
                CorrelationId = correlationId;
        }

        public Context(
              IModel data
            , ILocate locator = null
            , CancellationToken? token = null
            , string id = null
            , string correlationId = null)
            : this(locator)
        {
            Data.Add(data);

            if (token.HasValue)
                Token = token.Value;
            if (!string.IsNullOrEmpty(id))
                Id = id;
            if (!string.IsNullOrEmpty(correlationId))
                CorrelationId = correlationId;
        }
        public IList<IModel> Data { get; set; }
        public CancellationToken Token { get; set; }
        public string Id { get; set; }
        public string CorrelationId { get; set; }
        public ILocate Locator { get; }
    }
}
