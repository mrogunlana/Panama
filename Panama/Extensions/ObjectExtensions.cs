namespace Panama.Extensions
{
    public static class ObjectExtensions
    {
        public static int ToInt(this object? value)
        {
            if (value == null)
                return default(int);

            int.TryParse(string.Format("{0}", value), out int result);

            return result;
        }
    }
}
