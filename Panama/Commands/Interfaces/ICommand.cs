using Panama.Entities;
using System.Collections.Generic;

namespace Panama.Commands
{
    public interface ICommand
    {
        void Execute(List<IModel> data);
    }
}
