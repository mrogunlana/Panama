using Panama.Core.Entities;
using System.Collections.Generic;

namespace Panama.Core.Commands
{
    public interface IValidation
    {
        bool IsValid(List<IModel> data);
        string Message(List<IModel> data);
    }
}
