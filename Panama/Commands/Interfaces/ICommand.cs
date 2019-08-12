using Panama.Core.Entities;
using System.Collections.Generic;

namespace Panama.Core.Commands
{
    public interface ICommand
    {
        void Execute(List<IModel> data);
    }
}
