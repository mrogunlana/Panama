namespace Panama.Core.Extensions
{
    public static class StringExtensions
    {
        public static T ToEnum<T>(this string value)
            where T : struct
        {
            Enum.TryParse(value, out T result);

            return result;
        }
    }
}
