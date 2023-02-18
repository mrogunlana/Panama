using System.Threading.Tasks;

namespace Panama.Core.Interfaces
{
    public interface IValidate
    {
        Task Execute(IContext context);
        string Message(IContext context);
    }
}
