﻿using Microsoft.Extensions.DependencyInjection;
using Panama.Interfaces;

namespace Panama.Configuration
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
