namespace Panama.Core.Commands
{
    public interface IRollback
    {
        void Execute(Subject data);
    }
}
