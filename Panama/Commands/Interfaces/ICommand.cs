﻿namespace Panama.Core.Commands
{
    public interface ICommand
    {
        void Execute(Subject data);
    }
}
