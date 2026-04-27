using System;

namespace FifthSemester.Core.Services
{
    public interface IEventBus
    {
        void Publish<T>(T message);
        void Subscribe<T>(Action<T> handler);
        void Unsubscribe<T>(Action<T> handler);
    }
}