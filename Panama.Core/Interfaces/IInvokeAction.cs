using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Panama.Core.Interfaces
{
    public interface IInvokeAction
    {
        Task<IResult> Invoke<T>(IHandler handler) where T : IAction;
    }
}
