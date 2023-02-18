using System.Threading.Tasks;

namespace Panama.Core.Interfaces
{
    public interface ICommand
    {
        Task Execute(IContext context);
    }
}
