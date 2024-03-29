﻿using Panama.Interfaces;
using Panama.Models;

namespace Panama.Extensions
{
    public static class HandlerExtensions
    {
        public static IHandler AddKvp<K, V>(this IHandler handler, K key, V value)
        {
            if(handler.Context == null)
                throw new ArgumentNullException("Context could not be found on handler.");

            handler.Context.Data.Add(new Kvp<K, V>(key, value));
            
            return handler;
        }
    }
}
