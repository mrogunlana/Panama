using Panama.Interfaces;

namespace Panama.Canal.Brokers.Interfaces
{
    public interface IBrokerOptions : IModel
    {
        bool Default { get; set; }
        string Exchange { get; set; }
    }
}