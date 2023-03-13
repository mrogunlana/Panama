using Panama.Core.Interfaces;

namespace Panama.Core.Resolvers
{
    public delegate IInvoke<IHandler> HandlerInvokerResolver(string name);
}
