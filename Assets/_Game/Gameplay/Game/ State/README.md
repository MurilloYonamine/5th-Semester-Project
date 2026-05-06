# The `Game State` Directory: State Management Service

The **Game State** directory manages **high-level game flow**—transitions between Menu, Gameplay, Paused, Dialogue, and Cutscene states. All major gameplay decisions are gated by game state.

---

## Purpose

Game State Service provides:
- **Single source of truth** for "what is the game doing right now?"
- **Time scaling** (pause game by setting `Time.timeScale = 0`)
- **Event-driven state transitions** (publish events, not direct calls)
- **Input enabling/disabling** based on state

---

## State Machine

```
┌─ Menu ──────────────────┐
│   ↓                     │
│   Gameplay ─┬─ Dialogue │
│   ↑         │   ↓       │
│   └─ Paused ┴── ↑       │
│            Cutscene     │
└─────────────────────────┘
```

### State Transitions

| From | To | Trigger | Effect |
|------|-----|---------|--------|
| Menu | Gameplay | Player starts game | Unpause time |
| Gameplay | Paused | Player presses Esc | `Time.timeScale = 0` |
| Paused | Gameplay | Player resumes | `Time.timeScale = 1` |
| Gameplay | Dialogue | DialogueTrigger fires | Input disabled, dialogue shown |
| Dialogue | Gameplay | Dialogue ends | Input enabled, resume |
| Gameplay | Cutscene | Story event | Camera controlled, input disabled |

---

## Key File

### `GameStateService.cs`

Implements `IGameStateService` and manages all state transitions.

```csharp
public class GameStateService : MonoBehaviour, IGameStateService {
    public GameState CurrentState { get; set; }
    private GameState _previousState;
    private IEventBus _eventBus;

    public void ChangeState(GameState newState) {
        if (CurrentState == newState) return;

        _previousState = CurrentState;
        CurrentState = newState;

        // Time scaling
        if (CurrentState == GameState.Paused) {
            Time.timeScale = 0f;
        } else {
            Time.timeScale = 1f;
        }

        // Publish event for all listeners
        _eventBus.Publish(new GameStateChangedEvent(_previousState, CurrentState));
    }
}
```

---

## Usage

### Check Current State

```csharp
private void Update() {
    var gameState = ServiceLocator.Get<IGameStateService>();
    
    if (gameState.CurrentState == GameState.Gameplay) {
        // Handle gameplay updates
    } else if (gameState.CurrentState == GameState.Paused) {
        // Paused—don't move or animate
    }
}
```

### Transition to New State

```csharp
var gameState = ServiceLocator.Get<IGameStateService>();
gameState.ChangeState(GameState.Paused);
```

### Respond to State Changes

```csharp
private void Start() {
    _eventBus = ServiceLocator.Get<IEventBus>();
    _eventBus.Subscribe<GameStateChangedEvent>(OnGameStateChanged);
}

private void OnGameStateChanged(GameStateChangedEvent evt) {
    Debug.Log($"Game state changed from {evt.PreviousState} to {evt.CurrentState}");
    
    if (evt.CurrentState == GameState.Paused) {
        ShowPauseMenu();
    } else if (evt.CurrentState == GameState.Gameplay) {
        HidePauseMenu();
    }
}
```

---

## Event-Driven State Changes

Instead of directly calling `ChangeState()`, publish events and let GameStateService respond:

### Example: Starting Dialogue

```csharp
// In DialogueTrigger.cs
public void StartDialogue() {
    var eventBus = ServiceLocator.Get<IEventBus>();
    eventBus.Publish<DialogueStartedEvent>(new());
}

// In GameStateService.cs (listens)
private void OnDialogueStarted(DialogueStartedEvent evt) {
    ChangeState(GameState.Dialogue);
}
```

### Example: Pausing Game

```csharp
// In PauseMenu.cs
public void OnPauseButtonClicked() {
    var eventBus = ServiceLocator.Get<IEventBus>();
    eventBus.Publish<PauseToggleRequestedEvent>(new());
}

// In GameStateService.cs (listens)
private void OnPauseToggled(PauseToggleRequestedEvent evt) {
    if (CurrentState == GameState.Paused) {
        ChangeState(GameState.Gameplay);
    } else if (CurrentState == GameState.Gameplay) {
        ChangeState(GameState.Paused);
    }
}
```

---

## Time Scaling

### Paused State

When `CurrentState == GameState.Paused`, `Time.timeScale = 0`:
- Physics objects stop moving
- Animations pause
- Coroutines with `WaitForSeconds` pause
- BUT: `Update()` still runs (UI can be interactive)

### Dialogue State

When `CurrentState == GameState.Dialogue`, typically `Time.timeScale = 1`:
- World time continues flowing
- BUT: `InputService` disables player input
- Dialogue UI is interactive

---

## State-Based Input Control

### Input Enabling/Disabling

```csharp
// In InputService.cs
public void OnGameStateChanged(GameStateChangedEvent evt) {
    if (evt.CurrentState == GameState.Gameplay) {
        Enable();   // Player can move
    } else if (evt.CurrentState == GameState.Menu) {
        Disable();  // Player can't move in menu
    } else if (evt.CurrentState == GameState.Dialogue) {
        Disable();  // Dialogue has precedence
    } else if (evt.CurrentState == GameState.Paused) {
        Disable();  // Paused—no input
    }
}
```

---

## Best Practices

### 1. Use Events, Not Direct Calls

❌ **Bad:**
```csharp
var gameState = ServiceLocator.Get<IGameStateService>();
gameState.ChangeState(GameState.Paused);  // Direct call
```

✅ **Good:**
```csharp
var eventBus = ServiceLocator.Get<IEventBus>();
eventBus.Publish<PauseToggleRequestedEvent>(new());  // Event-driven
// GameStateService listens and transitions
```

**Why?** Events decouple—pause menu doesn't need to know about GameStateService directly.

### 2. Guard Against Redundant Transitions

✅ **Good:**
```csharp
public void ChangeState(GameState newState) {
    if (CurrentState == newState) return;  // Already in this state
    // ... proceed with transition
}
```

### 3. Validate State Transitions

✅ **Good:**
```csharp
public void ChangeState(GameState newState) {
    // Only allow certain transitions
    if (CurrentState == GameState.Menu && newState != GameState.Gameplay) {
        Debug.LogWarning("Invalid transition from Menu");
        return;
    }
    // ...
}
```

### 4. Document State Meanings

```csharp
public enum GameState {
    Gameplay,   // Player active in world
    MainMenu,   // Main menu screen
    Dialogue,   // Dialogue/conversation active
    Cutscene,   // Story cinematic (no input)
    Paused      // Game paused (Time.timeScale = 0)
}
```

### 5. Log State Transitions

✅ **Good:**
```csharp
Debug.Log($"{TAG} State: {_previousState} → {CurrentState}");
```

---

## Common Patterns

### Pause Menu Flow

```
Player presses Esc (InputService captures)
    ↓
Publishes PauseToggleRequestedEvent
    ↓
GameStateService listens
    ↓
ChangeState(GameState.Paused)
    ├─ Time.timeScale = 0
    └─ Publishes GameStateChangedEvent
    ↓
PauseMenuUI listens
    ├─ Shows pause menu
    └─ UI becomes interactive (Update still runs)
    ↓
Player clicks "Resume"
    ↓
Publishes PauseToggleRequestedEvent again
    ↓
ChangeState(GameState.Gameplay)
    ├─ Time.timeScale = 1
    └─ Publishes GameStateChangedEvent
    ↓
PauseMenuUI listens
    └─ Hides pause menu
    ↓
Game resumes
```

### Dialogue Flow

```
Player interacts with NPC (PlayerInteraction detects)
    ↓
Publishes DialogueStartedEvent
    ↓
GameStateService listens
    ├─ ChangeState(GameState.Dialogue)
    └─ Publishes GameStateChangedEvent
    ↓
InputService disables movement input
DialogueUI shows dialogue
    ↓
Player advances dialogue (clicks or presses button)
    ↓
Dialogue ends
    ├─ Publishes DialogueEndedEvent
    └─ GameStateService listens
    ↓
ChangeState(GameState.Gameplay)
    ├─ Time.timeScale = 1
    └─ Publishes GameStateChangedEvent
    ↓
InputService enables movement
DialogueUI hides
    ↓
Game resumes with player control
```

---

## Debugging State Transitions

### Print Current State

```csharp
var gameState = ServiceLocator.Get<IGameStateService>();
Debug.Log($"Current game state: {gameState.CurrentState}");
```

### Listen to All State Changes

```csharp
public class StateDebugger : MonoBehaviour {
    private void Start() {
        var eventBus = ServiceLocator.Get<IEventBus>();
        eventBus.Subscribe<GameStateChangedEvent>(OnStateChanged);
    }

    private void OnStateChanged(GameStateChangedEvent evt) {
        Debug.Log($"<color=cyan>STATE: {evt.PreviousState} → {evt.CurrentState}</color>");
    }
}
```

### Visualize Time Scaling

```csharp
private void OnGUI() {
    var gameState = ServiceLocator.Get<IGameStateService>();
    GUI.Label(new Rect(10, 10, 200, 20), 
        $"State: {gameState.CurrentState} (TimeScale: {Time.timeScale})");
}
```

---

## Advanced: Custom State Handling

### Add State-Specific Logic

```csharp
public void ChangeState(GameState newState) {
    // ... standard transition ...

    // State-specific cleanup/setup
    switch (newState) {
        case GameState.Paused:
            PauseAllAnimations();
            DisablePhysics();
            break;
        
        case GameState.Gameplay:
            ResumeAllAnimations();
            EnablePhysics();
            break;
        
        case GameState.Dialogue:
            DisablePlayerMovement();
            break;
    }
}
```

### State Entry/Exit Callbacks

```csharp
public class GameStateService : MonoBehaviour {
    public event System.Action<GameState> OnStateEnter;
    public event System.Action<GameState> OnStateExit;

    public void ChangeState(GameState newState) {
        OnStateExit?.Invoke(CurrentState);
        CurrentState = newState;
        OnStateEnter?.Invoke(newState);
        
        _eventBus.Publish(new GameStateChangedEvent(_previousState, CurrentState));
    }
}
```

---

## Summary

The **Game State** service provides:
- **Single state machine** for entire game
- **Event-driven transitions** (no tight coupling)
- **Time scaling** for pause functionality
- **Input gating** based on state
- **Clear game flow** control

By using GameStateService:
- **UI** knows when to show menus
- **Input** knows when to accept commands
- **Physics** knows when to simulate
- **Features** can respond to state changes via events

**See also:**
- [Gameplay/README.md](../README.md) - Gameplay feature overview
- [Core/Events/README.md](../../Core/Events/README.md) - Event system
