using Panama.Canal.Interfaces;
using Panama.Interfaces;

namespace Panama.Canal
{
    public class Subscriptions : IInvokeSubscriptions
    {
        public Task<IResult> Invoke(IContext? context = null)
        {
            throw new NotImplementedException();
        }
    }
}