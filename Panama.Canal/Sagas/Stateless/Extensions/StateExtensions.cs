using Panama.Canal.Sagas.Stateless.Interfaces;

namespace Panama.Canal.Sagas.Stateless.Extensions
{
    public static class StateExtensions
    {
        public static ISagaState Get<T>(this List<ISagaState> states)
            where T : ISagaState
        {
            var result = states.Get(typeof(T));
            if (result == null)
                throw new InvalidOperationException($"State of type: {typeof(T).Name} could not be located.");

            return result;
        }

        public static ISagaState? Get(this List<ISagaState> states, Type? type)
        {
            if (type == null)
                return null;
            if (states == null)
                throw new ArgumentNullException(nameof(states));

            var result = states.Where(x => x.GetType() == type).FirstOrDefault();

            return result;
        }
    }
}
