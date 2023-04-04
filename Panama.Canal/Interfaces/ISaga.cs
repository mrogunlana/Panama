using Panama.Interfaces;

namespace Panama.Canal.Interfaces
{
    public interface ISaga
    {
        Task<IResult> Start(IContext context);
        Task<IResult> Continue(IContext context);
    }
}