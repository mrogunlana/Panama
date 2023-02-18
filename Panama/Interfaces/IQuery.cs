using System.Threading.Tasks;

namespace Panama.Core.Interfaces
{
    public interface IQuery
    {
        Task Execute(IContext context);
    }
}
