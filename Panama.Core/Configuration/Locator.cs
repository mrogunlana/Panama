using Microsoft.Extensions.DependencyInjection;
using Panama.Core.Interfaces;
using System;
using System.Collections.Generic;

namespace Panama.Core.Configuration
{
    public class Locator : ILocate
    {
        private readonly IServiceProvider _serviceProvider;
        public Locator(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public T Resolve<T>()
        {
            return _serviceProvider.GetService<T>();
        }

        public IEnumerable<T> ResolveList<T>()
        {
            return _serviceProvider.GetServices<T>();
        }

        public T Resolve<T>(string name)
        {
            throw new NotImplementedException();
        }
    }
}
