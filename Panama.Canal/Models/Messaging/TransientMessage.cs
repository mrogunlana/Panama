﻿using Panama.Interfaces;
using System.Runtime.InteropServices;

namespace Panama.Canal.Models.Messaging
{
    [StructLayout(LayoutKind.Auto)]
    public readonly struct TransientMessage : IModel
    {
        public TransientMessage(IDictionary<string, string?> headers, ReadOnlyMemory<byte> body)
        {
            Headers = headers ?? throw new ArgumentNullException(nameof(headers));
            Body = body;
        }

        public IDictionary<string, string?> Headers { get; }

        public ReadOnlyMemory<byte> Body { get; }
    }
}
