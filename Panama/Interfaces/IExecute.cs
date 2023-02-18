using System.Threading.Tasks;

namespace Panama.Core.Interfaces
{
    public interface IExecute
    {
        Task Execute(IContext context);
    }
    public interface IExecute<T>
    {
        Task<T> Execute(IContext context);
    }
}
