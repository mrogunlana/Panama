using System.Threading.Tasks;

namespace Panama.Core.Interfaces
{
    public interface IAction
    {
        Task Execute(IContext context);
    }
}
