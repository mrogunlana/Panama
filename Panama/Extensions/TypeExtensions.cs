namespace Panama.Extensions
{
    public static class TypeExtensions
    {
        public static Type? GetGenericType<T>(this Type type)
        {
            var results = new List<Type>();
            foreach (var _type in typeof(T).GetType().GetInterfaces())
            {
                if (_type.IsGenericType && _type.GetGenericTypeDefinition() == type)
                {
                    results.Add(_type.GetGenericArguments()[0]);
                }
            }

            return results.FirstOrDefault();
        }
    }
}
