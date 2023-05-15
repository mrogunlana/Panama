using Panama.Interfaces;

namespace Panama.Canal.Tests.Models
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
