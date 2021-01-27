using System.Threading.Tasks;

namespace Panama.Core.Commands
{
    public interface IRollback
    {
        Task Execute(Subject subject);
    }
}
