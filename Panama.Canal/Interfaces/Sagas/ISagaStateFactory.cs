namespace Panama.Canal.Interfaces.Sagas
{
    public interface ISagaStateFactory
    {
        ISagaState Create<T>() where T : ISagaState;
    }
}