# The `Core` Directory: Global Services & Event Infrastructure

The **Core** layer is the backbone of PHOTOSSYNC. It provides essential, globally-accessible services that the entire game depends on—while maintaining **strict dependency rules**: **Features can depend on Core, but Core never depends on Features.**

This layer is built on proven patterns like **Service Locator** and **Event Bus** to ensure loose coupling and maximum testability.

---

## Architecture Overview

Core is divided into functional domains:

1. **`Services/` & ServiceLocator**: Centralized registry for all global services (Audio, Input, GameState, Dialogue, etc.)
2. **`Events/` & EventBus**: Decoupled communication channel for systems to publish and subscribe to events
3. **`Audio/`**: Unified audio management with channel support and mixer groups
4. **`Input/`**: Wrapper around Unity's new Input System for abstracted input handling
5. **`Enums/`**: Global enumeration types shared across the game
6. **`Initialization/`**: Bootstrap logic for registering and initializing all core services

---

## 1. Services (`Core/Services`)

### Purpose
Defines clean interfaces and provides a **Service Locator** for dependency injection without tight coupling.

### Key Files

#### `ServiceLocator.cs`
A static registry that stores and retrieves service implementations by their interface type.

**How it works:**
```csharp
// Register a service during initialization
ServiceLocator.Register<IAudioService>(audioServiceInstance);

// Request a service from anywhere in the game
IAudioService audio = ServiceLocator.Get<IAudioService>();
```

**Benefits:**
- No direct references needed between systems
- Services can be swapped easily for testing or alternative implementations
- Centralized access point—no hunting for singletons

### Service Interfaces

All services are defined as interfaces in this folder:

- **`IAudioService`**: Manages music, SFX, and audio channels
- **`IInputService`**: Abstracts controller and keyboard input
- **`IEventBus`**: Pub/Sub event system for decoupled communication
- **`IGameStateService`**: Tracks game state (Menu, Gameplay, Paused, GameOver)
- **`IGameplayService`**: Manages active gameplay rules and mechanics
- **`IMenuService`**: Handles menu navigation and transitions
- **`IDialogueService`**: Manages dialogue playback and character interactions
- **`IInventoryService`**: Tracks player items and equipment
- **`IPauseService`**: Centralized pause state management
- **`IScreenService`**: Handles screen resolution and graphics modes
- **`IGraphicsService`**: Manages visual settings and shader parameters
- **`ISettingsService`**: Persists user settings (audio levels, keybinds, etc.)

---

## 2. Events (`Core/Events`)

### Purpose
Provides a **Type-Safe Event Bus** for systems to communicate without direct coupling.

### Key Files

#### `EventBus.cs`
Central pub/sub system using C# generics and delegates.

**How it works:**
```csharp
// Get the EventBus from ServiceLocator
IEventBus eventBus = ServiceLocator.Get<IEventBus>();

// Publish an event (e.g., player died)
var deathEvent = new PlayerDiedEvent { PlayerHealth = 0 };
eventBus.Publish<PlayerDiedEvent>(deathEvent);

// Subscribe to events
eventBus.Subscribe<PlayerDiedEvent>(OnPlayerDied);

private void OnPlayerDied(PlayerDiedEvent evt)
{
    Debug.Log("Player is dead!");
}
```

**Benefits:**
- No runtime errors from missing subscribers or publishers
- Generic typing ensures type-safety
- Clean separation: UI, Audio, and Gameplay don't reference each other

#### `GlobalEvents.cs`
Defines core event types that multiple systems need to know about:
- Player state changes (spawn, death, health)
- Game state transitions (play, pause, menu)
- Inventory changes
- Dialogue events
- Enemy AI events

---

## 3. Audio (`Core/Audio`)

### Purpose
Centralized audio management with support for multiple channels, mixer groups, and dynamic volume control.

### Key Files

#### `AudioService.cs`
Implements the `IAudioService` interface for all audio needs.

**Features:**
- **Multiple Channels**: Separate audio channels for Music, SFX, Ambience, and Master control
- **Mixer Groups**: Integrates with Unity's AudioMixer for parameter-based volume/effects
- **Volume Control**: Individual control over each channel's volume
- **Track Transitions**: Smooth fade-in/fade-out between music tracks

**How it works:**
```csharp
IAudioService audio = ServiceLocator.Get<IAudioService>();

// Play music
audio.PlayMusic("MenuTheme", fadeIn: true);

// Play SFX
audio.PlaySFX("PlayerJump", volume: 1f);

// Control volume
audio.SetVolume(AudioChannel.Music, 0.8f);
```

#### `AudioChannel.cs` & `AudioTrack.cs`
Data structures representing individual audio channels and track metadata.

#### `MainMixer.mixer`
Unity AudioMixer asset containing:
- Master volume group
- Music, SFX, and Ambience submixes
- Mixer parameters for volume, pitch, and effects

---

## 4. Input (`Core/Input`)

### Purpose
Abstracts Unity's Input System to provide a clean, configurable input interface.

### Key Files

#### `GameInput.cs`
Implementation of `IInputService`—abstracts all player input.

**How it works:**
```csharp
IInputService input = ServiceLocator.Get<IInputService>();

// Check for input
if (input.IsActionPressed("Jump"))
{
    player.Jump();
}

// Get analog input
Vector2 movement = input.GetAnalogInput("Move");
```

#### `GameInput.inputactions`
Unity InputAction asset defining all game inputs (Jump, Move, Interact, Pause, etc.).
- Supports keyboard, gamepad, and mouse
- Easily rebindable by players

---

## 5. Enums (`Core/Enums`)

### Purpose
Global enumeration types used across multiple systems.

### Key Types

- **`GameState`**: Menu, Loading, Gameplay, Paused, GameOver
- **`Language`**: EN, PT, ES (for dialogue/UI localization)
- **`MenuScreen`**: MainMenu, Settings, Pause, Inventory, DialogueTree

---

## 6. Initialization (`Core/Initialization`)

### Purpose
Bootstrap sequence that registers all services and initializes Core systems.

**Typical initialization flow:**
```
1. Create ServiceLocator (implicit static initialization)
2. Create and register AudioService
3. Create and register InputService
4. Create and register EventBus
5. Create and register GameStateService
6. Create and register other specialized services (Dialogue, Inventory, Menu, etc.)
7. Signal "Core Ready" event
8. Allow Features to subscribe to events and request services
```

---

## Dependency Flow

```
┌─────────────────────────────────────────┐
│      Features (Gameplay, UI, Menu)      │
│  ↓ (depend on)                          │
│  ┌─────────────────────────────────────┤
│  │  Core (Services, Events, Audio)     │
│  │  ↑ (never depends on Features)      │
│  └─────────────────────────────────────┘
│
│  Communication Pattern:
│  Feature A --[EventBus]--> Feature B
│  (no direct coupling)
```

---

## Best Practices

### 1. Always Request Services via ServiceLocator
❌ **Bad:**
```csharp
public class PlayerController : MonoBehaviour
{
    private AudioService _audio = FindObjectOfType<AudioService>();
}
```

✅ **Good:**
```csharp
public class PlayerController : MonoBehaviour
{
    private IAudioService _audio = ServiceLocator.Get<IAudioService>();
}
```

### 2. Use EventBus for Feature-to-Feature Communication
❌ **Bad:**
```csharp
public class UIManager : MonoBehaviour
{
    public void OnPlayerDied()
    {
        gameOver.Show();
    }
}
// How does UIManager know when the player dies? Direct reference?
```

✅ **Good:**
```csharp
public class UIManager : MonoBehaviour
{
    private void Start()
    {
        IEventBus bus = ServiceLocator.Get<IEventBus>();
        bus.Subscribe<PlayerDiedEvent>(OnPlayerDied);
    }

    private void OnPlayerDied(PlayerDiedEvent evt)
    {
        gameOver.Show();
    }
}
```

### 3. Keep Core Free of Feature-Specific Logic
❌ **Bad:**
```csharp
// In Core/Audio/AudioService.cs
public void PlayEnemyAttackSound() { /* depends on Enemy feature */ }
```

✅ **Good:**
```csharp
// In Gameplay/Enemy/EnemyAttack.cs
public void Attack()
{
    IAudioService audio = ServiceLocator.Get<IAudioService>();
    audio.PlaySFX("EnemyAttack");
}
```

### 4. Initialize Services Deterministically
Ensure that all services are registered **before** any Feature tries to access them. Typically done in a single Bootstrap MonoBehaviour that runs first.

---

## Summary

The **Core** layer is the **glue** that holds PHOTOSSYNC together:

- **ServiceLocator** = Registry of dependencies
- **EventBus** = Decoupled inter-system messaging
- **Audio/Input** = Concrete service implementations
- **Enums** = Shared type definitions
- **Initialization** = Bootstrap logic

By strictly adhering to these patterns, the entire game remains **testable, maintainable, and easy to extend** without cascading changes across features.
