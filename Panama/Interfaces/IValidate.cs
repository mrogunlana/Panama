namespace Panama.Core.Interfaces
{
    public interface IValidate : IExecute<bool>
    {
        string Message(IContext context);
    }
}
