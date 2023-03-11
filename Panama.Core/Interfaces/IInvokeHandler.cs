using System.Threading.Tasks;

namespace Panama.Core.Interfaces
{
    public interface IInvokeHandler<T>
        where T : IHandler
    {
        Task<IResult> Invoke(T handler);
    }
}
