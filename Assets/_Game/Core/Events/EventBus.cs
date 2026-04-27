using System;
using System.Collections.Generic;
using FifthSemester.Core.Services;

namespace FifthSemester.Core.Events {

    public sealed class EventBus : IEventBus {
        private readonly Dictionary<Type, List<Delegate>> _subscriptions = new Dictionary<Type, List<Delegate>>();

        public void Publish<T>(T message) {
            Type messageType = typeof(T);

            if (!_subscriptions.TryGetValue(messageType, out List<Delegate> handlers)) {
                return;
            }

            Delegate[] snapshot = handlers.ToArray();

            for (int index = 0; index < snapshot.Length; index++) {
                ((Action<T>)snapshot[index])?.Invoke(message);
            }
        }

        public void Subscribe<T>(Action<T> handler) {
            if (handler == null) {
                throw new ArgumentNullException(nameof(handler));
            }

            Type messageType = typeof(T);

            if (!_subscriptions.TryGetValue(messageType, out List<Delegate> handlers)) {
                handlers = new List<Delegate>();
                _subscriptions.Add(messageType, handlers);
            }

            if (!handlers.Contains(handler)) {
                handlers.Add(handler);
            }
        }

        public void Unsubscribe<T>(Action<T> handler) {
            if (handler == null) {
                throw new ArgumentNullException(nameof(handler));
            }

            Type messageType = typeof(T);

            if (!_subscriptions.TryGetValue(messageType, out List<Delegate> handlers)) {
                return;
            }

            handlers.Remove(handler);

            if (handlers.Count == 0) {
                _subscriptions.Remove(messageType);
            }
        }
    }
}