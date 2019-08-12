using Ninject;

namespace Panama.Core.IoC.Ninject
{
    public class NinjectServiceLocator : IServiceLocator
    {
        private static IKernel _kernel = null;

        public NinjectServiceLocator(IKernel kernel)
        {
            _kernel = kernel;
        }

        public T Resolve<T>()
        {
            return _kernel.Get<T>();
        }

        public T Resolve<T>(string name)
        {
            return _kernel.Get<T>(name);
        }
    }
}
