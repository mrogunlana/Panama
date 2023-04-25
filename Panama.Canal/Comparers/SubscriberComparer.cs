using Microsoft.Extensions.Logging;
using Panama.Canal.Models.Descriptors;

namespace Panama.Canal.Comparers
{
    public class SubscriberComparer : IEqualityComparer<SubscriberDescriptor>
    {
        private readonly ILogger _log;

        public SubscriberComparer(ILogger log)
        {
            _log = log;
        }

        public bool Equals(SubscriberDescriptor? _this, SubscriberDescriptor? _that)
        {
            if (ReferenceEquals(_this, _that))
            {
                _log.LogWarning(
                    $"There are duplicate subscribers ({_this!.Topic}) in same group ({_this.Group}) and broker ({_this?.Target?.GetType()?.Name}), this may cause unintended results.");

                return true;
            }

            if (_this is null || _that is null) return false;

            var result = string.Equals(_this?.Topic, _that?.Topic, StringComparison.OrdinalIgnoreCase) &&
                         string.Equals(_this?.Group, _that?.Group, StringComparison.OrdinalIgnoreCase) &&
                         _this?.Target == _that?.Target;
            
            if (result)
                _log.LogWarning(
                    $"There are duplicate subscribers ({_this!.Topic}) in same group ({_this.Group}) and broker ({_this?.Target?.GetType()?.Name}), this may cause unintended results.");

            return result;
        }

        public int GetHashCode(SubscriberDescriptor? obj)
        {
            if (obj is null) return 0;

            var hashTopic = obj.Topic.GetHashCode();

            var hashGroup = obj.Group == null ? 0 : obj.Group.GetHashCode();

            var hashBroker = obj.Target == null ? 0 : obj.Target.GetHashCode();

            return hashGroup ^ hashTopic ^ hashBroker;
        }
    }
}
