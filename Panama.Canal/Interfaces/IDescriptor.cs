using Panama.Interfaces;

namespace Panama.Canal.Interfaces
{
    public interface IDescriptor : IModel
    {
        string Topic { get; set; }
        string Group { get; set; }
        Type Implementation { get; set; }
        Type Target { get; set; }
    }
}
