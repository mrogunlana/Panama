﻿namespace Panama.Interfaces
{
    public interface IAction : IModel
    {
        Task Execute(IContext context);
    }
}
