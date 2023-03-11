using System.Threading.Tasks;

namespace Panama.Core.Interfaces
{
    public interface IInvokeAction
    {
        Task<IResult> Invoke<T>(IHandler handler) where T : IAction;
    }
}
