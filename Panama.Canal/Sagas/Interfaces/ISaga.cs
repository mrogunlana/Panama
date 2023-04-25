using Panama.Canal.Sagas.Models;
using Panama.Interfaces;

namespace Panama.Canal.Sagas.Interfaces
{
    public interface ISaga
    {
        Type? Target { get; }
        string ReplyGroup { get; }
        string ReplyTopic { get; }
        Task Start(IContext context);
        Task<IResult> Continue(SagaContext context);
    }
}