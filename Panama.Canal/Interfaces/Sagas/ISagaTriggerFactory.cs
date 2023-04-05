namespace Panama.Canal.Interfaces.Sagas
{
    public interface ISagaTriggerFactory
    {
        ISagaTrigger Get<T>() where T : ISagaTrigger;
        ISagaTrigger Get(string type);
    }
}