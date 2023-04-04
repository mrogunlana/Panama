using Panama.Interfaces;

namespace Panama.Canal.Interfaces
{
    public interface IProcessor 
    {
        Task<IResult> Execute(IContext context);
    }
}