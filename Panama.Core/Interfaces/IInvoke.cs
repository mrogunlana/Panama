using System.Threading.Tasks;

namespace Panama.Core.Interfaces
{
    public interface IInvoke
    {
        Task<IResult> Invoke();
    }
    public interface IInvoke<T>
    {
        Task<IResult> Invoke(IHandler handler);
    }
}
