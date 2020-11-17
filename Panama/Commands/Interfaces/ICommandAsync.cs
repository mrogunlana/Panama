using System.Threading.Tasks;

namespace Panama.Core.Commands
{
    public interface ICommandAsync
    {
        Task Execute(Subject subject);
    }
}
