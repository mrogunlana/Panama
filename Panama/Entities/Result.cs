using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;

namespace Panama.Core.Entities
{
    public class Result : IResult
    {
        private HashSet<string> _messages;

        [OnSerialized]
        void OnSerialization(StreamingContext c)
        {
            _Init();
        }
        [OnDeserialized]
        void OnDeserialization(StreamingContext c)
        {
            _Init();
        }
        protected virtual void _Init()
        {
            if (Data == null)
                Data = new List<IModel>();

            _messages = new HashSet<string>();
        }
        public Result()
        {
            Success = true;
            _Init();
        }

        [DataMember]
        public List<IModel> Data { get; set; }

        [DataMember]
        public IEnumerable<string> Messages
        {
            get
            {
                return _messages;
            }
        }

        public void AddMessage(string message)
        {
            _messages.Add(message);
        }

        [DataMember]
        public bool Success { get; set; }

        internal static List<Type> DataContractKnownTypes()
        {
            List<Type> Ret = new List<Type>();

            Ret.AddRange(AppDomain.CurrentDomain
                .GetAssemblies()
                .SelectMany(x => GetLoadableTypes(x))
                .Where(x => typeof(IResult).IsAssignableFrom(x)
                    && !x.IsInterface
                    && !x.IsAbstract));

            Ret.AddRange(AppDomain.CurrentDomain
                .GetAssemblies()
                .SelectMany(x => GetLoadableTypes(x))
                .Where(x => typeof(IModel).IsAssignableFrom(x)
                    && !x.IsInterface
                    && !x.IsAbstract));

            return Ret;
        }

        internal static IEnumerable<Type> GetLoadableTypes(Assembly assembly)
        {
            try
            {
                return assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException e)
            {
                return e.Types.Where(t => t != null);
            }
        }
    }
}
