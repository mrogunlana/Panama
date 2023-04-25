using Panama.Canal.Models.Messaging;

namespace Panama.Canal.Brokers
{
    public class UnsubscriptionObserver : IDisposable
    {
        private List<IObserver<InternalMessage>> _observers;
        private IObserver<InternalMessage> _observer;

        public UnsubscriptionObserver(List<IObserver<InternalMessage>> observers,
                            IObserver<InternalMessage> observer)
        {
            _observers = observers;
            _observer = observer;
        }

        public void Dispose()
        {
            if (_observer == null)
                return;

            _observers.Remove(_observer);
        }
    }
}