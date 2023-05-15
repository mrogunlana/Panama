namespace Panama.Canal.Sagas.Stateless.Interfaces
{
    public interface ISagaStateFactory
    {
        ISagaState Create<T>() where T : ISagaState;
    }
}