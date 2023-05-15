using Panama.Interfaces;

namespace Panama.Canal.Extensions
{
    public static class ResultExtensions
    {
        public static IResult Published<T>(this IResult result, T model)
            where T : IModel
        {
            result.Data.Published(model);

            return result;
        }

        public static IResult Queue<T>(this IResult result, T model)
            where T : IModel
        {
            result.Data.Queue(model);

            return result;
        }
    }
}
