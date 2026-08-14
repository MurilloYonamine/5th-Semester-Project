using UnityEngine;
using FifthSemester.Core.Services;
using FifthSemester.Core.Events;
using FifthSemester.Core.Input;

namespace FifthSemester.Gameplay {
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
            ServiceLocator.Register<IInputService>(inputService);

            var inventoryService = new InventoryService(maxCapacity: 6);
            ServiceLocator.Register<IInventoryService<Item>>(inventoryService);

            var itemRegistry = new ItemRegistry<Item>();
            ServiceLocator.Register<IItemRegistry<Item>>(itemRegistry);

            var settingsService = new SettingsService();
            ServiceLocator.Register<ISettingsService>(settingsService);

            var saveService = new SaveService();
            ServiceLocator.Register<ISaveService>(saveService);

            var hudService = new HUDService();
            ServiceLocator.Register<IHUDService>(hudService);

            GameObject coreSystems = Resources.Load<GameObject>(CORE_SYSTEMS);
            if (coreSystems != null) {
                GameObject instantiateObject = Object.Instantiate(coreSystems);
                instantiateObject.name = "[ CORE SYSTEMS ]";
                Object.DontDestroyOnLoad(instantiateObject);
                Debug.Log($"{TAG} Core systems initialized successfully.");
            }
            else {
                Debug.LogError($"{TAG} Failed to load core systems prefab at path: {CORE_SYSTEMS}");
            }

            var menuService = new MenuService();
            ServiceLocator.Register<IMenuService>(menuService);
        }
    }
}
