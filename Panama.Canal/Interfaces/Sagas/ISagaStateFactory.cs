namespace Panama.Canal.Interfaces.Sagas
{
    public interface ISagaStateFactory
    {
        ISagaState Get<T>() where T : ISagaState;
    }
}