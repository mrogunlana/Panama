using Microsoft.Extensions.Logging;
using Panama.Canal.Models;

namespace Panama.Canal.Comparers
{
    public class SubscriptionComparer : IEqualityComparer<Subscription>
    {
        private readonly ILogger _log;

        public SubscriptionComparer(ILogger log)
        {
            _log = log;
        }

        public bool Equals(Subscription? _this, Subscription? _that)
        {
            if (ReferenceEquals(_this, _that))
            {
                _log.LogWarning($"There are duplicate subscribers ({_this!.Topic}) in same group ({_this.Group}), this may cause unintended results.");
                
                return true;
            }

            if (_this is null || _that is null) return false;

            var result = _this.Topic.Equals(_that.Topic, StringComparison.OrdinalIgnoreCase) &&
                         _this.Group.Equals(_that.Group, StringComparison.OrdinalIgnoreCase);

            if (result)
                _log.LogWarning(
                    $"There are duplicate subscribers ({_this!.Topic}) in same group ({_this.Group}), this may cause unintended results.");

            return result;
        }

        public int GetHashCode(Subscription? obj)
        {
            if (obj is null) return 0;

            var hashGroup = obj.Group == null ? 0 : obj.Group.GetHashCode();

            var hashTopic = obj.Topic.GetHashCode();

            return hashGroup ^ hashTopic;
        }
    }
}
