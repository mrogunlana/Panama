﻿using System.Threading.Tasks;

namespace Panama.Core.Interfaces
{
    public interface IInvoke
    {
        Task<IResult> Invoke(IContext context = null);
    }
    public interface IInvoke<T>
    {
        Task<IResult> Invoke(IContext context);
    }
}
