using Panama.Interfaces;
using System.Reflection;
using System.Runtime.Serialization;

namespace Panama.Models
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
        public IList<IModel> Data { get; set; }

        [DataMember]
        public IList<string> Messages
        {
            get
            {
                return _messages.ToList();
            }
        }

        public void AddMessage(string message)
        {
            _messages.Add(message);
        }

        public void AddMessages(IEnumerable<string> messages)
        {
            foreach (var message in messages)
                _messages.Add(message);
        }

        [DataMember]
        public bool Success { get; set; }

        [DataMember]
        public bool Cancelled { get; set; }

        internal static List<Type> DataContractKnownTypes()
        {
            //TODO: Needs to use common registrar here for assemblies

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
