namespace Panama.Canal.Extensions
{
    public static class ObjectExtensions
    {
        public static T? To<T>(this object? value)
        {
            if (value == null)
                return default(T);
            
            if (value is T)
                return (T?)value;

            return default(T);
        }
    }
}
