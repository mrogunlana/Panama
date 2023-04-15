﻿using Panama.Canal.Brokers.Interfaces;
using Panama.Canal.Models;

namespace Panama.Canal.Brokers
{
    public class DefaultObservable : IDefaultObservable
    {
        private List<IObserver<InternalMessage>> _observers;
        public DefaultObservable()
        {
            _observers = new List<IObserver<InternalMessage>>();
        }
        public void Publish(InternalMessage message)
        {
            foreach (var observer in _observers)
                observer.OnNext(message);
        }

        public IDisposable Subscribe(IObserver<InternalMessage> observer)
        {
            if (!_observers.Contains(observer))
                _observers.Add(observer);

            return new DefaultUnsubscriber(_observers, observer);
        }
    }
}
