using System.Threading.Tasks;

namespace Panama.Core.Interfaces
{
    public interface IRollback
    {
        Task Execute(IContext context);
    }
}
