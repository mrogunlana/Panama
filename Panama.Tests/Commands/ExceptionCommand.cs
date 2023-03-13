using Panama.Commands;

namespace Panama.Tests.Commands
{
    public class ExceptionCommand : ICommand
    {
        public void Execute(Subject subject)
        {
            throw new System.Exception();
        }
    }
}
