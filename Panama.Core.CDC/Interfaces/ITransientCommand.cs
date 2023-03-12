using Panama.Core.Interfaces;

namespace Panama.Core.CDC.Interfaces
{
    public interface ITransientCommand<T> 
        : ICommand where T : ITransactionProvider { }
}
