using Panama.Interfaces;

namespace Panama.Canal.Tests.Modules.Models
{
    public class Foo : IModel
    {
        public string? Value { get; set; }
        public Foo()
        {
            Value = DateTime.UtcNow.ToShortDateString();
        }
    }
}
