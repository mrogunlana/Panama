using Panama.Core.Commands;

namespace Panama.Core.Tests.Commands
{
    public class ExceptionCommand : ICommand
    {
        public void Execute(Subject subject)
        {
            throw new System.Exception();
        }
    }
}
