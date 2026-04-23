using UnityEngine;
using FifthSemester.Core.Services;
using FifthSemester.Core.Events;

namespace FifthSemester.Core
{
    public static class GameBootstrapper
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        public static void ResetDomain()
        {
            ServiceLocator.Clear();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void Initialize()
        {
            var eventBus = new EventBus();

            ServiceLocator.Register<IEventBus>(eventBus);
        }
    }
}