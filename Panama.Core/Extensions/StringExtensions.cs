using System;

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

        public static int ToInt(this object value)
        {
            if (value == null)
                return default(int);

            int.TryParse(string.Format("{0}", value), out int result);

            return result;
        }
    }
}
