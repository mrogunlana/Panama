using System.Threading.Tasks;

namespace Panama.Core.Interfaces
{
    public interface IInvoke
    {
        Task<IResult> Invoke(IContext context);
    }
}
