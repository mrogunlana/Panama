using Microsoft.Extensions.Logging;
using Panama.Interfaces;

namespace Panama.Configuration
{
    public class LogFactory : ILogFactory
    {
        private readonly ILoggerFactory _factory;
        public LogFactory(ILoggerFactory logger)
        {
            _factory = logger;
        }

        public ILog<T> CreateLogger<T>()
        {
            return new Logger<T>(_factory.CreateLogger<T>());
        }
    }
}
