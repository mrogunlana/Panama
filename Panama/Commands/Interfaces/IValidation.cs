using Panama.Entities;
using System.Collections.Generic;

namespace Panama.Commands
{
    public interface IValidation
    {
        bool IsValid(List<IModel> data);
        string Message();
    }
}
