namespace Panama.Core.Interfaces
{
    public interface IHandler : IInvoke 
    {
        IContext Context { get; }
        IHandler Query<Query>() where Query : IQuery;
        IHandler Command<Command>() where Command : ICommand;
        IHandler Rollback<Rollback>() where Rollback : IRollback;
        IHandler Validate<Validate>() where Validate : IValidate;
        IHandler Set<Invoker>() where Invoker : IInvoke<IHandler>;
        IHandler Add(IModel data);
        IHandler Add(CancellationToken token);
        IHandler Add(params IModel[] data);
        IHandler Add(IEnumerable<IModel> data);
    }
}
