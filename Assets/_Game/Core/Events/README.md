# The `Events` Directory: Pub/Sub Event System

The **Events** layer implements a **Type-Safe Event Bus** that allows systems to communicate without direct coupling. Instead of Feature A directly calling Feature B, Feature A publishes an event that Feature B can subscribe to—if needed.

---

## Architecture Overview

The event system is built on these principles:

1. **EventBus**: Central pub/sub registry (implements `IEventBus`)
2. **GlobalEvents**: Shared event types that multiple systems care about
3. **Event Structure**: Readonly structs containing event data
4. **Subscribers**: Systems that listen for events via `IEventBus.Subscribe<T>()`

---

## Key Files

### `EventBus.cs` (IEventBus Implementation)

The central message broker for the entire game.

#### How It Works

```csharp
// 1. Get the EventBus
IEventBus eventBus = ServiceLocator.Get<IEventBus>();

// 2. Subscribe to an event type
eventBus.Subscribe<PlayerDiedEvent>(OnPlayerDied);

// 3. Publish an event
var evt = new PlayerDiedEvent { PlayerHealth = 0 };
eventBus.Publish<PlayerDiedEvent>(evt);

// 4. Handler gets invoked
private void OnPlayerDied(PlayerDiedEvent evt)
{
    Debug.Log($"Player died with 0 health");
}

// 5. Unsubscribe when done
eventBus.Unsubscribe<PlayerDiedEvent>(OnPlayerDied);
```

#### Key Methods

```csharp
// Subscribe to an event
void Subscribe<T>(Action<T> handler);

// Publish an event
void Publish<T>(T message);

// Unsubscribe from an event
void Unsubscribe<T>(Action<T> handler);
```

#### Internal Structure

- Uses `Dictionary<Type, List<Delegate>>` to store subscriptions
- Generic typing ensures compile-time type safety
- Prevents duplicate subscriptions automatically
- Null-safe: unsubscribing from non-existent subscriptions is safe

---

### `GlobalEvents.cs`

Defines **event types** that multiple systems need to know about. These are the "public protocol" of the game.

#### Event Types

##### Game State Events

```csharp
/// Fired when the game transitions between Menu -> Gameplay -> Paused -> GameOver
public readonly struct GameStateChangedEvent {
    public readonly GameState PreviousState;
    public readonly GameState CurrentState;
}
```

##### Input Events

Represent raw input from the player:

```csharp
/// Movement input (analog stick or WASD)
public readonly struct MoveInputEvent {
    public readonly Vector2 Value;
}

/// Look/camera input (analog stick or mouse)
public readonly struct LookInputEvent {
    public readonly Vector2 Value;
}

/// Jump button pressed
public readonly struct JumpInputEvent { }

/// Crouch button (pressed/held)
public readonly struct CrouchInputEvent {
    public readonly bool IsPressed;
}

/// Sprint button (pressed/held)
public readonly struct SprintInputEvent {
    public readonly bool IsPressed;
}
```

#### Event Design Pattern

All events are **readonly structs**:

```csharp
public readonly struct EventName {
    public readonly Type Property1;
    public readonly Type Property2;

    public EventName(Type prop1, Type prop2) {
        Property1 = prop1;
        Property2 = prop2;
    }
}
```

**Why readonly struct?**
- Zero heap allocation (stack-based)
- Immutable—prevents accidental data corruption
- Lightweight—perfect for high-frequency events
- Clear contract: subscribers cannot modify event data

---

### `InputService.cs` (IInputService Implementation)

Listens for input events and translates them into game-relevant state changes.

#### Purpose

- Responds to `GameStateChangedEvent` to enable/disable input based on game state
- Bridges Unity's Input System (`GameInput.cs`) with the EventBus

#### Key Methods

```csharp
// Enable/disable all input
void Enable();
void Disable();

// React to game state changes
void OnGameStateChanged(GameStateChangedEvent evt);

// Current game state property
GameState CurrentGameState { get; set; }
```

#### Typical Flow

```
GameStateService publishes GameStateChangedEvent
    ↓
InputService listens and updates CurrentGameState
    ↓
If state == Gameplay → InputService publishes MoveInputEvent, JumpInputEvent, etc.
    ↓
Player/Controller systems listen and respond
```

---

## Usage Patterns

### 1. Subscribe to an Event (in a MonoBehaviour)

```csharp
public class UIManager : MonoBehaviour
{
    private IEventBus _eventBus;

    private void Start()
    {
        _eventBus = ServiceLocator.Get<IEventBus>();
        _eventBus.Subscribe<GameStateChangedEvent>(OnGameStateChanged);
    }

    private void OnDestroy()
    {
        _eventBus.Unsubscribe<GameStateChangedEvent>(OnGameStateChanged);
    }

    private void OnGameStateChanged(GameStateChangedEvent evt)
    {
        if (evt.CurrentState == GameState.GameOver)
        {
            gameOverPanel.SetActive(true);
        }
    }
}
```

### 2. Publish an Event from a Feature

```csharp
public class PlayerHealth : MonoBehaviour
{
    private IEventBus _eventBus;

    private void Start()
    {
        _eventBus = ServiceLocator.Get<IEventBus>();
    }

    public void TakeDamage(int damage)
    {
        _health -= damage;

        if (_health <= 0)
        {
            // Notify all subscribers
            var evt = new PlayerDiedEvent { PlayerHealth = _health };
            _eventBus.Publish<PlayerDiedEvent>(evt);
        }
    }
}
```

### 3. Define a New Event Type

Add to `GlobalEvents.cs`:

```csharp
public readonly struct InventoryUpdatedEvent {
    public readonly int ItemId;
    public readonly int Quantity;

    public InventoryUpdatedEvent(int itemId, int quantity) {
        ItemId = itemId;
        Quantity = quantity;
    }
}
```

Then use it:

```csharp
// Publish
var evt = new InventoryUpdatedEvent(itemId: 5, quantity: 3);
_eventBus.Publish<InventoryUpdatedEvent>(evt);

// Subscribe
_eventBus.Subscribe<InventoryUpdatedEvent>(OnInventoryUpdated);

private void OnInventoryUpdated(InventoryUpdatedEvent evt)
{
    Debug.Log($"Added {evt.Quantity} of item {evt.ItemId}");
}
```

### 4. One-Time Event Listening (Manual Unsubscribe)

```csharp
public class DialogueUI : MonoBehaviour
{
    private IEventBus _eventBus;

    public void ShowDialogue(string dialogueId)
    {
        _eventBus = ServiceLocator.Get<IEventBus>();

        // Subscribe
        _eventBus.Subscribe<DialogueFinishedEvent>(OnDialogueFinished);
    }

    private void OnDialogueFinished(DialogueFinishedEvent evt)
    {
        // Unsubscribe immediately after handling
        _eventBus.Unsubscribe<DialogueFinishedEvent>(OnDialogueFinished);

        dialoguePanel.SetActive(false);
    }
}
```

---

## Best Practices

### 1. Always Define Events as Readonly Structs

❌ **Bad:**
```csharp
public class PlayerDiedEvent
{
    public int Health { get; set; }  // Mutable!
}
```

✅ **Good:**
```csharp
public readonly struct PlayerDiedEvent
{
    public readonly int Health;

    public PlayerDiedEvent(int health) {
        Health = health;
    }
}
```

### 2. Unsubscribe in OnDestroy

❌ **Bad:**
```csharp
private void Start()
{
    _eventBus.Subscribe<GameStateChangedEvent>(OnStateChanged);
    // If this object is destroyed, the handler stays registered!
}
```

✅ **Good:**
```csharp
private void Start()
{
    _eventBus.Subscribe<GameStateChangedEvent>(OnStateChanged);
}

private void OnDestroy()
{
    _eventBus.Unsubscribe<GameStateChangedEvent>(OnStateChanged);
}
```

### 3. Don't Create Events That Mirror Method Calls

❌ **Bad:**
```csharp
// Don't do this—just call directly
public readonly struct PlayAudioEvent { }

// Instead of:
_eventBus.Publish<PlayAudioEvent>(new());

// Publisher would:
_eventBus.Publish<PlayAudioEvent>(new());
```

✅ **Good:**
```csharp
// Use events for genuine state changes, not actions
public readonly struct PlayerDiedEvent
{
    public readonly int FinalHealth;
}
```

### 4. Keep Event Payloads Simple

❌ **Bad:**
```csharp
public readonly struct InventoryChangedEvent
{
    public readonly Player Player;                // Unnecessary coupling
    public readonly List<Item> AllInventoryItems; // Overkill
}
```

✅ **Good:**
```csharp
public readonly struct InventoryChangedEvent
{
    public readonly int ItemId;
    public readonly int Quantity;
}
```

### 5. Centralize Event Definitions

❌ **Bad:**
```csharp
// Scattered event definitions everywhere
// → Hard to see what events exist
// → Namespace collisions
```

✅ **Good:**
```csharp
// All events in GlobalEvents.cs
// → Single source of truth
// → Clear game protocol
```

---

## Event Flow Example: Player Death

```
Player Health reaches 0
    ↓
PlayerHealth.TakeDamage() publishes PlayerDiedEvent
    ↓
EventBus.Publish<PlayerDiedEvent>(evt)
    ↓
All subscribers receive notification:
    ├─ UIManager.OnPlayerDied() → shows GameOver panel
    ├─ AudioService.OnPlayerDied() → plays death sound
    ├─ CameraSystem.OnPlayerDied() → zooms camera
    ├─ EnemyAI.OnPlayerDied() → stops attacking
    └─ GameStateService.OnPlayerDied() → transitions to GameOver state
    ↓
No direct coupling—each system is independent
```

---

## Extending the Event System

### To Add a New Event Type:

1. **Define in `GlobalEvents.cs`**:
```csharp
public readonly struct BossDefeatedEvent {
    public readonly int BossId;
    public readonly int RewardGold;

    public BossDefeatedEvent(int bossId, int rewardGold) {
        BossId = bossId;
        RewardGold = rewardGold;
    }
}
```

2. **Publish from Feature**:
```csharp
// In Boss.cs
var evt = new BossDefeatedEvent(bossId: 42, rewardGold: 500);
_eventBus.Publish<BossDefeatedEvent>(evt);
```

3. **Subscribe from Other Features**:
```csharp
// In UIRewards.cs
_eventBus.Subscribe<BossDefeatedEvent>(OnBossDefeated);

// In InventoryService.cs
_eventBus.Subscribe<BossDefeatedEvent>(OnBossDefeated);
```

---

## Summary

The **Events** subsystem provides:
- **Decoupled communication** between systems
- **Type-safe pub/sub** messaging with generics
- **Zero-allocation events** using readonly structs
- **Centralized event definitions** in `GlobalEvents.cs`
- **Clean separation** of concerns

By publishing events instead of calling features directly, PHOTOSSYNC remains **flexible, testable, and maintainable**.
