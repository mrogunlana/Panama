using System.Threading.Tasks;

namespace Panama.Core.Interfaces
{
    public interface IAction : IModel
    {
        Task Execute(IContext context);
    }
}
