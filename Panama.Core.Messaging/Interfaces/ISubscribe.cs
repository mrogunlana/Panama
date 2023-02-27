﻿using System.Collections.Generic;
using System.Threading;

namespace Panama.Core.Messaging.Interfaces
{
    public interface ISubscribe<T> where T : IBroker
    {
        string To { get; }
    }
}