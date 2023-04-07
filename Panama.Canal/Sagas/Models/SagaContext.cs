using Panama.Canal.Interfaces;
using Panama.Interfaces;
using Panama.Models;

namespace Panama.Canal.Sagas.Models
{
    public class SagaContext : Context
    {
        public Type? Type { get; set; }
        public IContext? Origin { get; set; }
        public IChannel? Channel { get; set; }
        public SagaContext(
            IServiceProvider provider,
            IContext? origin = null)
            : base(provider)
        {
            Origin = origin;
        }
    }
}
