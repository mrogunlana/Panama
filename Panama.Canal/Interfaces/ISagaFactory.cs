using Panama.Canal.Models;
using Panama.Interfaces;

namespace Panama.Canal.Interfaces
{
    public interface ISagaFactory
    {
        Task<ISaga> StartSaga<S>(IChannel channel, IEnumerable<IModel> models) where S : ISaga;
        Task<ISaga> StartSaga<S>(IChannel channel, params IModel[] models) where S : ISaga;
        Task<ISaga> GetSaga(InternalMessage message);
    }
}