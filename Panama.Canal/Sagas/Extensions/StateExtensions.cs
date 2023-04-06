using Panama.Canal.Interfaces.Sagas;

namespace Panama.Canal.Sagas.Extensions
{
    public static class StateExtensions
    {
        public static ISagaState Get<T>(this List<ISagaState> states)
            where T : ISagaState
        {
            if (states == null)
                throw new ArgumentNullException(nameof(states));
            
            var result = states.Where(x => x is T).FirstOrDefault();
            if (result == null)
                throw new InvalidOperationException($"State of type: {typeof(T).Name} could not be located.");

            return result;
        }
    }
}
