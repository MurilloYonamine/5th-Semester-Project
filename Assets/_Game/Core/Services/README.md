# The `Services` Directory: Service Interfaces & Locator

The **Services** directory defines all the **service contracts** (interfaces) that features depend on, plus the **ServiceLocator** pattern that manages their registration and retrieval. This ensures loose coupling and centralized access to global functionality.

---

## Architecture Overview

The Services directory implements the **Service Locator** pattern:

1. **ServiceLocator.cs**: Static registry that stores and retrieves services by interface type
2. **Service Interfaces**: All `IXxxService` interfaces defining contracts for core functionality
3. **Implementations**: Concrete implementations (often in sibling folders like `Audio/`, `Input/`, etc.)

```
┌─────────────────────────────────┐
│ Features (Player, Enemy, UI)    │
│ ↓ (depend on)                   │
├─────────────────────────────────┤
│ ServiceLocator.Get<IXxxService> │
│ ↑ (returns)                     │
├─────────────────────────────────┤
│ Services/ (interfaces)          │
│ ↑ (implemented by)              │
├─────────────────────────────────┤
│ Audio/, Input/, etc.            │
│ (concrete implementations)      │
└─────────────────────────────────┘
```

---

## Key Files

### `ServiceLocator.cs`

Static registry for all services in the game. Provides centralized dependency injection without tight coupling.

#### How It Works

```csharp
// 1. Register a service (typically during Core initialization)
var audioService = new AudioService();
ServiceLocator.Register<IAudioService>(audioService);

// 2. Request a service from anywhere
IAudioService audio = ServiceLocator.Get<IAudioService>();
audio.PlaySFX("Jump");

// 3. Unregister when done (cleanup)
ServiceLocator.Unregister<IAudioService>();

// 4. Clear all services (usually on shutdown)
ServiceLocator.Clear();
```

#### Implementation Details

```csharp
public static class ServiceLocator
{
    private static readonly Dictionary<Type, object> _services = new();

    // Register a service by its interface type
    public static void Register<T>(T service) { ... }

    // Retrieve a service by its interface type
    public static T Get<T>() { ... }

    // Unregister a specific service
    public static void Unregister<T>() { ... }

    // Clear all registered services
    public static void Clear() { ... }
}
```

#### Benefits

- **No Singletons**: Avoid the fragility of MonoBehaviour singletons
- **Easy Testing**: Mock implementations can replace real services
- **Centralized**: Single location to manage all global dependencies
- **Type-Safe**: Compile-time checking via generics
- **Late Binding**: Services can be registered/unregistered at runtime

---

## Service Interfaces

### `IAudioService`

Manages audio playback, channels, and volume control.

```csharp
public interface IAudioService
{
    // Play one-shot SFX
    AudioSource PlaySFX(string filePath, AudioMixerGroup mixer = null, ...);
    AudioSource PlaySFX(AudioClip clip, AudioMixerGroup mixer = null, ...);

    // Play looping tracks
    AudioTrack PlayTrack(AudioClip clip, int channel = 0, ...);
    AudioTrack PlayAmbience(AudioClip clip, int channel = 0, ...);

    // Stop audio
    void StopTrack(int channelNumber);
    void StopTrack(string trackName);
    void StopAllTracks();
    void StopSFX(AudioClip clip);
    void StopSFX(string sfxName);
    void StopAllSFX();

    // Volume control
    void SetMasterVolume(float volume, bool muted = false);
    void SetMusicVolume(float volume, bool muted = false);
    void SetSFXVolume(float volume, bool muted = false);
    void SetAmbienceVolume(float volume, bool muted = false);
}
```

**See**: [Core/Audio/README.md](../Audio/README.md)

---

### `IInputService`

Manages input handling and state awareness.

```csharp
public interface IInputService
{
    GameState CurrentGameState { get; set; }
    
    void Enable();
    void Disable();

    void OnGameStateChanged(GameStateChangedEvent evt);
}
```

**See**: [Core/Input/README.md](../Input/README.md)

---

### `IEventBus`

Publishes and subscribes to typed events.

```csharp
public interface IEventBus
{
    // Subscribe to event type
    void Subscribe<T>(Action<T> handler);

    // Publish event
    void Publish<T>(T message);

    // Unsubscribe from event
    void Unsubscribe<T>(Action<T> handler);
}
```

**See**: [Core/Events/README.md](../Events/README.md)

---

### `IGameStateService`

Manages high-level game state (Menu, Gameplay, Paused, etc.).

```csharp
public interface IGameStateService
{
    GameState CurrentState { get; }

    // Transition to a new state
    void ChangeState(GameState newState);

    // Check if in a specific state
    bool IsInState(GameState state);
}
```

#### Usage

```csharp
IGameStateService gameState = ServiceLocator.Get<IGameStateService>();

// Get current state
if (gameState.CurrentState == GameState.Gameplay)
{
    // Game is active
}

// Transition
gameState.ChangeState(GameState.Paused);

// Event-driven transition
IEventBus eventBus = ServiceLocator.Get<IEventBus>();
eventBus.Publish<GameStateChangedEvent>(
    new(GameState.Gameplay, GameState.Paused)
);
```

---

### `IGameplayService`

Manages active gameplay rules and mechanics.

```csharp
public interface IGameplayService
{
    // Gameplay-specific methods (implemented by your game logic)
    void StartLevel(int levelId);
    void EndLevel(bool victory);
    void PauseGameplay();
    void ResumeGameplay();
}
```

#### Usage

```csharp
IGameplayService gameplay = ServiceLocator.Get<IGameplayService>();
gameplay.StartLevel(1);
```

---

### `IMenuService`

Handles menu navigation and transitions.

```csharp
public interface IMenuService
{
    MenuScreen CurrentScreen { get; }

    // Navigate to a screen
    void NavigateTo(MenuScreen screen);

    // Go back to previous screen
    void GoBack();

    // Show/hide menu
    void ShowMenu();
    void HideMenu();
}
```

#### Usage

```csharp
IMenuService menu = ServiceLocator.Get<IMenuService>();

menu.NavigateTo(MenuScreen.Settings);
menu.NavigateTo(MenuScreen.Settings_Audio);
menu.GoBack();  // Back to Settings
menu.GoBack();  // Back to MainMenu
```

---

### `IDialogueService`

Manages dialogue playback and character interactions.

```csharp
public interface IDialogueService
{
    // Start dialogue
    void PlayDialogue(string dialogueId);

    // End current dialogue
    void StopDialogue();

    // Get current dialogue state
    bool IsDialoguePlaying { get; }
}
```

#### Usage

```csharp
IDialogueService dialogue = ServiceLocator.Get<IDialogueService>();
dialogue.PlayDialogue("npc_greeting_001");
```

---

### `IInventoryService`

Tracks player items and equipment.

```csharp
public interface IInventoryService
{
    // Add/remove items
    void AddItem(int itemId, int quantity = 1);
    void RemoveItem(int itemId, int quantity = 1);

    // Query inventory
    int GetItemQuantity(int itemId);
    bool HasItem(int itemId);

    // Equipment
    void EquipItem(int itemId);
    void UnequipItem(int itemId);
    int GetEquippedItem();
}
```

#### Usage

```csharp
IInventoryService inventory = ServiceLocator.Get<IInventoryService>();
inventory.AddItem(itemId: 1, quantity: 5);

if (inventory.HasItem(2))
{
    inventory.EquipItem(2);
}
```

---

### `IPauseService`

Centralized pause state management.

```csharp
public interface IPauseService
{
    bool IsPaused { get; }

    void Pause();
    void Resume();
    void TogglePause();
}
```

#### Usage

```csharp
IPauseService pause = ServiceLocator.Get<IPauseService>();

if (!pause.IsPaused)
{
    pause.Pause();
}
```

---

### `IScreenService`

Handles screen resolution and graphics modes.

```csharp
public interface IScreenService
{
    // Resolution management
    void SetResolution(int width, int height, bool fullscreen);
    Vector2Int CurrentResolution { get; }

    // Display mode
    void SetFullscreen(bool fullscreen);
    bool IsFullscreen { get; }
}
```

#### Usage

```csharp
IScreenService screen = ServiceLocator.Get<IScreenService>();
screen.SetResolution(1920, 1080, fullscreen: true);
```

---

### `IGraphicsService`

Manages visual settings and shader parameters.

```csharp
public interface IGraphicsService
{
    // Graphics quality
    void SetGraphicsQuality(int level);  // 0=Low, 1=Medium, 2=High
    int CurrentQuality { get; }

    // Shader parameters
    void SetShaderParameter(string name, float value);
    void SetShaderParameter(string name, Color color);
}
```

#### Usage

```csharp
IGraphicsService graphics = ServiceLocator.Get<IGraphicsService>();
graphics.SetGraphicsQuality(2);  // High quality
graphics.SetShaderParameter("PixelationAmount", 0.5f);
```

---

### `ISettingsService`

Persists user settings (volume, keybinds, preferences).

```csharp
public interface ISettingsService
{
    // Volume settings
    float MasterVolume { get; set; }
    float MusicVolume { get; set; }
    float SFXVolume { get; set; }
    float AmbienceVolume { get; set; }

    // Language
    Language CurrentLanguage { get; set; }

    // Save/Load
    void SaveSettings();
    void LoadSettings();
}
```

#### Usage

```csharp
ISettingsService settings = ServiceLocator.Get<ISettingsService>();
settings.MasterVolume = 0.8f;
settings.CurrentLanguage = Language.English;
settings.SaveSettings();
```

---

## Typical Core Initialization Flow

Services are registered in a specific order to respect dependencies:

```csharp
public class CoreBootstrapper : MonoBehaviour
{
    private void Awake()
    {
        // 1. Event system (needed by everything)
        var eventBus = new EventBus();
        ServiceLocator.Register<IEventBus>(eventBus);

        // 2. Settings (needed by audio, graphics, etc.)
        var settings = new SettingsService();
        ServiceLocator.Register<ISettingsService>(settings);
        settings.LoadSettings();

        // 3. Core services
        var audioService = gameObject.AddComponent<AudioService>();
        ServiceLocator.Register<IAudioService>(audioService);

        var inputService = gameObject.AddComponent<InputService>();
        ServiceLocator.Register<IInputService>(inputService);

        var gameStateService = new GameStateService();
        ServiceLocator.Register<IGameStateService>(gameStateService);

        // 4. Gameplay services
        var menuService = new MenuService();
        ServiceLocator.Register<IMenuService>(menuService);

        var dialogueService = new DialogueService();
        ServiceLocator.Register<IDialogueService>(dialogueService);

        var inventoryService = new InventoryService();
        ServiceLocator.Register<IInventoryService>(inventoryService);

        // Signal that Core is ready
        eventBus.Publish<CoreInitializedEvent>(new());
    }
}
```

---

## Best Practices

### 1. Always Use Interfaces, Never Concrete Implementations

❌ **Bad:**
```csharp
public class Player : MonoBehaviour
{
    private AudioService _audio;  // Concrete class
}
```

✅ **Good:**
```csharp
public class Player : MonoBehaviour
{
    private IAudioService _audio = ServiceLocator.Get<IAudioService>();
}
```

### 2. Request Services in Start(), Not Awake()

❌ **Bad:**
```csharp
private void Awake()
{
    _audio = ServiceLocator.Get<IAudioService>();  // Might not be registered yet
}
```

✅ **Good:**
```csharp
private void Start()
{
    _audio = ServiceLocator.Get<IAudioService>();  // Core is guaranteed initialized
}
```

### 3. Handle Null Services Gracefully

✅ **Good:**
```csharp
private void Start()
{
    _audio = ServiceLocator.Get<IAudioService>();

    if (_audio == null)
    {
        Debug.LogWarning("AudioService not initialized");
        return;
    }
}
```

### 4. Use Dependency Injection for Testing

✅ **Good:**
```csharp
public class Player : MonoBehaviour
{
    private IAudioService _audio;

    public void SetAudioService(IAudioService audioService)
    {
        _audio = audioService;  // Testable—inject mock
    }
}

// In tests:
var mockAudio = new MockAudioService();
player.SetAudioService(mockAudio);
```

### 5. Keep Service Interfaces Focused

❌ **Bad:**
```csharp
public interface IAudioService
{
    // Too many responsibilities
    void PlaySFX(...);
    void SetGraphicsQuality(...);  // Unrelated!
    void SaveSettings(...);        // Unrelated!
}
```

✅ **Good:**
```csharp
public interface IAudioService
{
    void PlaySFX(...);
    void PlayTrack(...);
    void SetVolume(...);
    // All audio-related
}

public interface IGraphicsService
{
    void SetGraphicsQuality(...);
}

public interface ISettingsService
{
    void SaveSettings(...);
}
```

---

## Service Dependency Graph

```
ISettingsService
    ↑ (used by)
├── IAudioService
├── IGraphicsService
└── IScreenService

IEventBus
    ↑ (used by)
├── IGameStateService
├── IInputService
├── IMenuService
└── IDialogueService

IGameStateService
    ↑ (used by)
├── IInputService
└── IPauseService

IInputService
    ↑ (used by)
└── Player & Features
```

---

## Adding a New Service

### Step 1: Define Interface

Create `INewService.cs` in `Services/`:

```csharp
public interface INewService
{
    void DoSomething();
    int GetValue();
}
```

### Step 2: Implement Interface

Create implementation in appropriate folder (e.g., `Core/NewFeature/NewService.cs`):

```csharp
public class NewService : MonoBehaviour, INewService
{
    private void Awake()
    {
        ServiceLocator.Register<INewService>(this);
    }

    public void DoSomething() { ... }
    public int GetValue() { ... }
}
```

### Step 3: Use from Features

```csharp
private void Start()
{
    var service = ServiceLocator.Get<INewService>();
    service.DoSomething();
}
```

---

### `ISaveService`

Serviço centralizado de persistência usando `PlayerPrefs` (padrão Resident Evil checkpoints).

```csharp
public interface ISaveService {
    void SaveToSlot(string slotId, SaveData data);
    SaveData LoadFromSlot(string slotId);
    void DeleteSlot(string slotId);
    bool SlotExists(string slotId);
    string[] ListSlots();
    void SaveCheckpoint(string checkpointId, SaveData data);

    event Action<string> OnSaveCompleted;
}

[System.Serializable]
public class SaveData {
    public int CurrentMissionIndex;
    public Dictionary<string, string> MissionProgress;
    public string LastCheckpointId;
    public int SaveVersion;
    public long Timestamp;
}
```

#### Uso Típico

```csharp
// Salvar estado atual
var saveService = ServiceLocator.Get<ISaveService>();
var saveData = new SaveData { CurrentMissionIndex = 2 };
saveService.SaveToSlot("default", saveData);

// Carregar
SaveData loaded = saveService.LoadFromSlot("default");
Debug.Log($"Mission Index: {loaded.CurrentMissionIndex}");

// Checkpoint
saveService.SaveCheckpoint("library_save", saveData);
```

#### PlayerPrefs Storage
- Keys: `save_{slotId}` → JSON serializado
- Exemplo: `save_default`, `save_checkpoint_1`
- Gerenciado automaticamente pelo `SaveService`

---

## Summary

The **Services** directory provides:
- **Service Locator** for centralized dependency management
- **Interface definitions** for all core services
- **Loose coupling** between systems (depend on interfaces, not implementations)
- **Easy testing** (mock services for unit tests)
- **Single responsibility** (each service has a focused purpose)
- **Type-safe access** (generics ensure compile-time correctness)

By adhering to Service Locator pattern, PHOTOSSYNC achieves **flexibility, testability, and maintainability**.
