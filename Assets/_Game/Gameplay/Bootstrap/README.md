# The `Bootstrap` Directory: Game Initialization

The **Bootstrap** directory contains the **startup logic** that initializes all Gameplay services before any scene loads. It runs via `[RuntimeInitializeOnLoadMethod]` to ensure Core systems are ready.

---

## Purpose

Bootstrap executes **before any scene loads**, ensuring:

1. **ServiceLocator is cleared** (fresh state per play session)
2. **Core systems are registered** (EventBus, InputService, SettingsService, etc.)
3. **Gameplay services are registered** (InventoryService, MenuService, etc.)
4. **Core systems prefab is instantiated** (marked with `DontDestroyOnLoad`)
5. All features can access services via `ServiceLocator.Get<IXxxService>()`

---

## Key File

### `GameBootstrapper.cs`

Static class with static methods decorated with `[RuntimeInitializeOnLoadMethod]`.

```csharp
[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
public static void ResetDomain() {
    ServiceLocator.Clear();  // Clean slate for new play session
}

[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
public static void Initialize() {
    // Register all services
}
```

#### Initialization Sequence

```
1. ResetDomain() [SubsystemRegistration]
   ├─ Clear ServiceLocator
   └─ Prepare fresh state

2. Initialize() [BeforeSceneLoad]
   ├─ Create and register EventBus
   ├─ Create and register InputService
   ├─ Create and register InventoryService
   ├─ Create and register SettingsService
   ├─ Instantiate Core Systems prefab (DontDestroyOnLoad)
   ├─ Create and register MenuService
   └─ Log success/errors
```

---

## Services Registered

### Event System
```csharp
var eventBus = new EventBus();
ServiceLocator.Register<IEventBus>(eventBus);
```

### Input Handling
```csharp
var inputService = new InputService();
inputService.Enable();
ServiceLocator.Register<IInputService>(inputService);
```

### Inventory Management
```csharp
var inventoryService = new InventoryService(maxCapacity: 6);
ServiceLocator.Register<IInventoryService<Item>>(inventoryService);
```

### User Settings
```csharp
var settingsService = new SettingsService();
ServiceLocator.Register<ISettingsService>(settingsService);
```

### Core Systems Prefab
```csharp
GameObject coreSystems = Resources.Load<GameObject>("[ CORE SYSTEMS ]");
if (coreSystems != null) {
    GameObject instantiateObject = Object.Instantiate(coreSystems);
    Object.DontDestroyOnLoad(instantiateObject);
}
```

**What's in `[ CORE SYSTEMS ].prefab`:**
- AudioService component
- GraphicsService component
- ScreenService component
- Other persistent Core systems

### Menu Service
```csharp
var menuService = new MenuService();
ServiceLocator.Register<IMenuService>(menuService);
```

---

## Prefab Setup

### `[ CORE SYSTEMS ].prefab`

Located at: `Assets/_Game/Gameplay/Bootstrap/Resources/[ CORE SYSTEMS ].prefab`

**Components:**
- AudioService (plays music, SFX, ambience)
- GraphicsService (manages visual settings)
- ScreenService (handles resolution, fullscreen)
- GameStateService (manages high-level state)
- Any other persistent Core systems

**Why DontDestroyOnLoad?**
These systems must persist across scene transitions:
- Menu → Gameplay → GameOver → Menu

If they were destroyed, audio would stop, settings would reset, etc.

---

## How It Works

### Timing

```
Unity Engine Start
    ↓
[RuntimeInitializeLoadType.SubsystemRegistration]
    ├─ GameBootstrapper.ResetDomain()
    └─ ServiceLocator.Clear()
    ↓
[RuntimeInitializeLoadType.BeforeSceneLoad]
    ├─ GameBootstrapper.Initialize()
    ├─ Services registered
    └─ Core Systems prefab instantiated
    ↓
Scene Loads
    ├─ MonoBehaviours instantiated
    ├─ Start() methods called
    ├─ Features can now access services
    └─ Game begins
```

### Service Access

After Bootstrap completes, any script can request services:

```csharp
private void Start() {
    // Services are guaranteed to exist now
    var audio = ServiceLocator.Get<IAudioService>();
    var input = ServiceLocator.Get<IInputService>();
    var inventory = ServiceLocator.Get<IInventoryService<Item>>();
}
```

---

## Configuration

### Adjusting Inventory Capacity

```csharp
// In GameBootstrapper.Initialize()
var inventoryService = new InventoryService(maxCapacity: 8);  // Change to 8
```

### Loading Custom Core Systems Prefab

```csharp
// Modify the path
const string CORE_SYSTEMS = "[ CORE SYSTEMS ]";  // Change if renamed

// Or load from different path
GameObject coreSystems = Resources.Load<GameObject>("CustomPath/MyCoreSystems");
```

### Adding New Gameplay Services

```csharp
// Add registration
var myNewService = new MyNewService();
ServiceLocator.Register<IMyNewService>(myNewService);

// Now available everywhere
var service = ServiceLocator.Get<IMyNewService>();
```

---

## Best Practices

### 1. Keep Bootstrap Minimal

❌ **Bad:**
```csharp
// Bootstrap does too much
public static void Initialize() {
    // ... register 20 services ...
    // ... complex initialization logic ...
    // ... scene loading ...
}
```

✅ **Good:**
```csharp
// Bootstrap only registers services
public static void Initialize() {
    CreateAndRegisterEventBus();
    CreateAndRegisterInputService();
    CreateAndRegisterInventoryService();
    // ...
}
```

### 2. Verify Service Registration

✅ **Good:**
```csharp
var audio = ServiceLocator.Get<IAudioService>();
if (audio == null) {
    Debug.LogError("AudioService failed to register");
}
```

### 3. Handle Missing Prefab Gracefully

✅ **Good:**
```csharp
GameObject coreSystems = Resources.Load<GameObject>(CORE_SYSTEMS);
if (coreSystems == null) {
    Debug.LogError($"Failed to load core systems prefab");
    return;  // Or create a fallback
}
```

### 4. Log Bootstrap Progress

✅ **Good:**
```csharp
Debug.Log($"{TAG} EventBus initialized");
Debug.Log($"{TAG} InputService initialized");
Debug.Log($"{TAG} Core systems prefab instantiated");
Debug.Log($"{TAG} Bootstrap complete");
```

### 5. Don't Register in Bootstrap if Not Necessary

❌ **Bad:**
```csharp
// Too early to register—Player doesn't exist yet
var player = FindObjectOfType<Player>();
ServiceLocator.Register<Player>(player);
```

✅ **Good:**
```csharp
// Player registers itself when it's created
public class Player : MonoBehaviour {
    private void Start() {
        // Can access services now
        var audio = ServiceLocator.Get<IAudioService>();
    }
}
```

---

## Common Issues

### Issue: "Service not found" in Start()

**Cause**: Accessing service before Bootstrap completes.

**Solution**: Move access to `Start()` or later, not `Awake()`.

```csharp
// ❌ Bad: Awake is too early
private void Awake() {
    var audio = ServiceLocator.Get<IAudioService>();  // Might not exist yet
}

// ✅ Good: Start is guaranteed after Bootstrap
private void Start() {
    var audio = ServiceLocator.Get<IAudioService>();
}
```

### Issue: Core Systems prefab not found

**Cause**: Prefab path incorrect or file doesn't exist.

**Solution**: Verify prefab location and path:
```
Assets/_Game/Gameplay/Bootstrap/Resources/[ CORE SYSTEMS ].prefab
```

Check that the exact name matches in code.

### Issue: Services persist across scenes unexpectedly

**Cause**: DontDestroyOnLoad applied incorrectly.

**Solution**: Only apply to Core Systems prefab, not individual features.

---

## Testing Bootstrap

### Manual Test

1. Create a new scene
2. Add a test script that accesses services
3. Play the scene
4. Verify services are available:

```csharp
public class BootstrapTest : MonoBehaviour {
    private void Start() {
        var eventBus = ServiceLocator.Get<IEventBus>();
        var inputService = ServiceLocator.Get<IInputService>();
        var inventory = ServiceLocator.Get<IInventoryService<Item>>();
        
        if (eventBus != null && inputService != null && inventory != null) {
            Debug.Log("✓ Bootstrap successful");
        } else {
            Debug.LogError("✗ Bootstrap failed");
        }
    }
}
```

### Unit Test

```csharp
[Test]
public void TestBootstrapRegistersServices() {
    GameBootstrapper.Initialize();
    
    var eventBus = ServiceLocator.Get<IEventBus>();
    var inputService = ServiceLocator.Get<IInputService>();
    
    Assert.IsNotNull(eventBus);
    Assert.IsNotNull(inputService);
}
```

---

## Flow Diagram

```
┌────────────────────────────────────┐
│ Play Game / Enter Play Mode        │
└────────────────────────────────────┘
                ↓
┌────────────────────────────────────┐
│ [RuntimeInitializeOnLoadMethod]    │
│ SubsystemRegistration             │
│                                    │
│ GameBootstrapper.ResetDomain()    │
│   └─ ServiceLocator.Clear()       │
└────────────────────────────────────┘
                ↓
┌────────────────────────────────────┐
│ [RuntimeInitializeOnLoadMethod]    │
│ BeforeSceneLoad                   │
│                                    │
│ GameBootstrapper.Initialize()     │
│   ├─ Create EventBus              │
│   ├─ Create InputService          │
│   ├─ Create InventoryService      │
│   ├─ Create SettingsService       │
│   ├─ Instantiate Core Systems     │
│   └─ Create MenuService           │
└────────────────────────────────────┘
                ↓
┌────────────────────────────────────┐
│ Unity Scene Loading               │
│ (First scene in Build Settings)   │
└────────────────────────────────────┘
                ↓
┌────────────────────────────────────┐
│ MonoBehaviours Awake/Start        │
│ Features access services          │
│ Game begins                       │
└────────────────────────────────────┘
```

---

## Summary

The **Bootstrap** directory provides:
- **Single-point initialization**: All services registered in one place
- **Guaranteed order**: Services initialized before any feature runs
- **Persistence**: Core Systems prefab survives scene transitions
- **Error handling**: Logging and validation of service registration
- **Clean startup**: Fresh ServiceLocator state per play session

By centralizing Bootstrap logic, PHOTOSSYNC ensures:
- **Consistent startup** across all play sessions
- **No missing services** when features start
- **Easy debugging** of initialization issues
- **Flexibility** to add new services as the game grows

**See also:**
- [Core/Services/README.md](../../Core/Services/README.md) - How services work
- [Gameplay/Game State/README.md](../Game%20State/README.md) - State management after Bootstrap
