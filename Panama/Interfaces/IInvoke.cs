using System.Threading.Tasks;

namespace Panama.Core.Interfaces
{
    public interface IInvoke<T> where T : IExecute
    {
        Task<IResult> Invoke(IHandler handler);
    }
}
