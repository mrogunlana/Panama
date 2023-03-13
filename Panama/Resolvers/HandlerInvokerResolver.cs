using Panama.Interfaces;

namespace Panama.Resolvers
{
    public delegate IInvoke<IHandler> HandlerInvokerResolver(string name);
}
