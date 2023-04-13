using Panama.Canal.Interfaces;

namespace Panama.Canal.Brokers.Interfaces
{
    public interface ITargetFactory
    {
        ITarget GetDefaultTarget();
    }
}