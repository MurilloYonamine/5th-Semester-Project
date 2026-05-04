# The `Inicialization` Directory: Core Bootstrap

The **Initialization** directory is reserved for the **bootstrap sequence** that sets up the entire Core layer during game startup. This is where all services are created, registered with ServiceLocator, and prepared for use by Features.

---

## Purpose

While currently empty, this directory should contain:

1. **CoreBootstrapper.cs** or **GameInitializer.cs**: Main bootstrap MonoBehaviour
2. **ServiceInitializer.cs**: Helper for registering services in order
3. **ConfigurationLoader.cs**: Load initial settings from files or ScriptableObjects

---

## Typical Initialization Sequence

When the game starts, Core should initialize in this order:

```
1. EventBus → All systems need this to communicate
2. SettingsService → Load user preferences
3. AudioService → Initialize audio system with settings
4. InputService → Activate input handling
5. GameStateService → Set initial state to MainMenu
6. MenuService → Show main menu
7. Other services as needed...
```

This ensures **dependencies are satisfied** before systems try to use them.

---

## Example Bootstrap Pattern

When implementing `CoreBootstrapper.cs`, follow this structure:

```csharp
public class CoreBootstrapper : MonoBehaviour
{
    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        InitializeCore();
    }

    private void InitializeCore()
    {
        // 1. Event system first (everything depends on this)
        InitializeEventBus();

        // 2. Settings (needed by audio, graphics, etc.)
        InitializeSettingsService();

        // 3. Audio system
        InitializeAudioService();

        // 4. Input system
        InitializeInputService();

        // 5. Game state
        InitializeGameStateService();

        // 6. Other services
        InitializeMenuService();
        InitializeDialogueService();
        InitializeInventoryService();

        // 7. Signal readiness
        PublishCoreReadyEvent();

        Debug.Log("Core initialization complete");
    }

    private void InitializeEventBus()
    {
        var eventBus = new EventBus();
        ServiceLocator.Register<IEventBus>(eventBus);
    }

    private void InitializeSettingsService()
    {
        var settings = new SettingsService();
        ServiceLocator.Register<ISettingsService>(settings);
        settings.LoadSettings();
    }

    private void InitializeAudioService()
    {
        var audio = gameObject.AddComponent<AudioService>();
        ServiceLocator.Register<IAudioService>(audio);
    }

    // ... etc for other services ...

    private void PublishCoreReadyEvent()
    {
        var eventBus = ServiceLocator.Get<IEventBus>();
        eventBus.Publish<CoreInitializedEvent>(new());
    }
}
```

---

## Setup Checklist

When creating `Initialization/CoreBootstrapper.cs`:

- [ ] Create CoreBootstrapper MonoBehaviour
- [ ] Add to a bootstrap scene that loads first
- [ ] Mark with `DontDestroyOnLoad()` so services persist across scenes
- [ ] Initialize services in dependency order
- [ ] Register each service with ServiceLocator
- [ ] Call service initialization methods (e.g., `LoadSettings()`)
- [ ] Publish `CoreInitializedEvent` when done
- [ ] Log success/errors for debugging

---

## Initialization Event

Define a signal event that fires when Core is ready:

```csharp
// In Core/Events/GlobalEvents.cs
public readonly struct CoreInitializedEvent { }
```

Features can then listen for this and initialize themselves:

```csharp
// In Gameplay/Player/PlayerManager.cs
private void Start()
{
    IEventBus eventBus = ServiceLocator.Get<IEventBus>();
    eventBus.Subscribe<CoreInitializedEvent>(OnCoreReady);
}

private void OnCoreReady(CoreInitializedEvent evt)
{
    Debug.Log("Core ready, initializing Player");
    // Initialize player-specific logic
}
```

---

## Best Practices

### 1. Initialize in Dependency Order

✅ **Good:**
```
1. EventBus (needed by everything)
2. SettingsService (needed by Audio, Graphics)
3. AudioService (depends on SettingsService)
4. GameStateService (may publish events)
```

❌ **Bad:**
```
Initialize AudioService before SettingsService
→ Audio can't load user volume preferences
→ Game starts loud despite mute setting
```

### 2. Use DontDestroyOnLoad for Core

✅ **Good:**
```csharp
private void Awake()
{
    DontDestroyOnLoad(gameObject);
    InitializeCore();
}
```

This keeps Core services alive across scene transitions.

### 3. Handle Missing Service Gracefully

✅ **Good:**
```csharp
private void InitializeAudioService()
{
    try
    {
        var audio = gameObject.AddComponent<AudioService>();
        ServiceLocator.Register<IAudioService>(audio);
        Debug.Log("AudioService initialized");
    }
    catch (Exception ex)
    {
        Debug.LogError($"Failed to initialize AudioService: {ex}");
    }
}
```

### 4. Log Initialization Progress

✅ **Good:**
```csharp
private void InitializeCore()
{
    Debug.Log("=== Core Initialization Started ===");

    InitializeEventBus();
    Debug.Log("✓ EventBus initialized");

    InitializeSettingsService();
    Debug.Log("✓ SettingsService initialized");

    // ... etc ...

    Debug.Log("=== Core Initialization Complete ===");
}
```

### 5. Test Service Availability

✅ **Good:**
```csharp
private void VerifyCoreInitialization()
{
    var eventBus = ServiceLocator.Get<IEventBus>();
    var audio = ServiceLocator.Get<IAudioService>();
    var input = ServiceLocator.Get<IInputService>();

    if (eventBus == null || audio == null || input == null)
    {
        Debug.LogError("Core initialization incomplete!");
        return false;
    }

    Debug.Log("Core verified: all services available");
    return true;
}
```

---

## Integration with Features

### After Core Initializes

Features should follow this pattern:

```csharp
public class PlayerManager : MonoBehaviour
{
    private void Start()
    {
        // Services are guaranteed available after Core bootstrap
        var audio = ServiceLocator.Get<IAudioService>();
        var input = ServiceLocator.Get<IInputService>();
        var eventBus = ServiceLocator.Get<IEventBus>();

        // Listen to events
        eventBus.Subscribe<GameStateChangedEvent>(OnGameStateChanged);

        // Initialize feature
        InitializePlayer();
    }
}
```

### Test without Full Core

For unit tests, you can partially initialize:

```csharp
[Test]
public void TestPlayerWithoutCore()
{
    // Create only the services we need
    var mockEventBus = new MockEventBus();
    var mockAudio = new MockAudioService();

    ServiceLocator.Register<IEventBus>(mockEventBus);
    ServiceLocator.Register<IAudioService>(mockAudio);

    // Test player logic
    var player = new Player();
    player.Jump();

    // Verify
    Assert.IsTrue(mockEventBus.PublishedEvent<JumpInputEvent>());
}
```

---

## Scene Setup

### Bootstrap Scene

Create a scene called `Bootstrap.unity` or `Core.unity`:

```
Scene: Bootstrap
├── CoreBootstrapper (GameObject)
│   ├── AudioService (Component)
│   ├── InputService (Component)
│   ├── GameStateService (Script)
│   └── ... other services ...
```

### Game Scene (Gameplay)

Load this after Bootstrap:

```
Scene: MainMenu
├── UIManager (uses ServiceLocator to get services)
├── MenuController (subscribes to events)
└── ... gameplay objects ...
```

Scene transition:

```csharp
// Bootstrap scene loads first
SceneManager.LoadScene("Bootstrap", LoadSceneMode.Single);

// Then load gameplay
SceneManager.LoadScene("MainMenu", LoadSceneMode.Additive);
```

---

## Common Issues & Solutions

### Issue: "Service not found" on startup

**Cause**: Feature tries to access service before Core initializes.

**Solution**:
```csharp
// ❌ Bad: Accessing in Awake
private void Awake()
{
    var audio = ServiceLocator.Get<IAudioService>();  // Too early!
}

// ✅ Good: Accessing in Start
private void Start()
{
    var audio = ServiceLocator.Get<IAudioService>();  // Core definitely initialized
}
```

### Issue: Settings not loading on startup

**Cause**: Settings service not in bootstrap sequence.

**Solution**: Ensure `InitializeSettingsService()` is called **before** `InitializeAudioService()`:

```csharp
private void InitializeCore()
{
    InitializeEventBus();           // 1st
    InitializeSettingsService();    // 2nd (load settings FIRST)
    InitializeAudioService();       // 3rd (apply loaded settings)
}
```

### Issue: Input not responding in menu

**Cause**: Input service not initialized or game state not set.

**Solution**: Initialize input service AND set initial game state:

```csharp
private void InitializeCore()
{
    InitializeInputService();
    InitializeGameStateService();
    
    // Set initial state to MainMenu
    var gameState = ServiceLocator.Get<IGameStateService>();
    gameState.ChangeState(GameState.MainMenu);
}
```

---

## Summary

The **Initialization** directory is reserved for:
- **Bootstrap logic** that creates and registers all Core services
- **Initialization helpers** for managing service setup
- **Configuration loading** from files or ScriptableObjects
- **Startup verification** to ensure all systems are ready

By centralizing initialization in one location, PHOTOSSYNC ensures:
- **Consistent startup** across all runs
- **Correct dependency order** (no missing services)
- **Easy debugging** (see what initialized when)
- **Testability** (mock services for unit tests)

When implementing Core bootstrap, follow the **dependency order** and always use **ServiceLocator** to register and retrieve services.
