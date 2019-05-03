using Autofac;

namespace Panama.IoC.Autofac
{
    public class AutofacServiceLocator : IServiceLocator
    {
        private static IContainer _kernel = null;

        public AutofacServiceLocator(IContainer kernel)
        {
            _kernel = kernel;
        }

        public T Resolve<T>()
        {
            return _kernel.Resolve<T>();
        }

        public T Resolve<T>(string name)
        {
            return _kernel.ResolveNamed<T>(name);
        }
    }
}
