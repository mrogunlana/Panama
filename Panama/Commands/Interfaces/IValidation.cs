namespace Panama.Core.Commands
{
    public interface IValidation
    {
        bool IsValid(Subject data);
        string Message(Subject data);
    }
}
