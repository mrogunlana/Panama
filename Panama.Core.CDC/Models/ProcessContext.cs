using Panama.Core.Interfaces;
using Panama.Core.Models;

namespace Panama.Core.CDC.Models
{
    public class ProcessContext : Context
    {
        public ProcessContext(
              ILocate locator
            , CancellationToken? token = null)
            : base(locator)
        {
            if (token.HasValue)
                Token = token.Value;
        }
    }
}
