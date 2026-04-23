using UnityEngine;
using FifthSemester.Core.Services;
using FifthSemester.Core.Events;
using FifthSemester.Core.Audio;
using FifthSemester.Gameplay.Inventory;

namespace FifthSemester.Gameplay.Bootstrap {
    public static class GameBootstrapper {
        private const string TAG = "<color=cyan>[GameBootstrapper]</color> ";
        private const string CORE_SYSTEMS = "[ CORE SYSTEMS ]";

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        public static void ResetDomain() {
            ServiceLocator.Clear();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void Initialize() {
            var eventBus = new EventBus();
            ServiceLocator.Register<IEventBus>(eventBus);

            var inputService = new InputService();
            inputService.Enable();
            ServiceLocator.Register<InputService>(inputService);

            var inventoryService = new InventoryService(maxCapacity: 6);
            ServiceLocator.Register<IInventoryService<Item>>(inventoryService);

            GameObject coreSystems = Resources.Load<GameObject>(CORE_SYSTEMS);
            if (coreSystems != null) {
                Object.Instantiate(coreSystems);
                Debug.Log($"{TAG} Core systems initialized successfully.");
            }
            else {
                Debug.LogError($"{TAG} Failed to load core systems prefab at path: {CORE_SYSTEMS}");
            }
        }
    }
}