using System.Threading.Tasks;

namespace Panama.Core.Interfaces
{
    public interface IInvokeResult<T>
    {
        Task<IResult> Invoke(IHandler handler);
    }
}
