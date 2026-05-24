# The `Gameplay` Directory: Game Features & Mechanics

The **Gameplay** layer contains all **game-specific features** that implement PHOTOSSYNC's core mechanics. Each subdirectory is a self-contained feature with scripts, assets, and data.

This layer depends on Core and Framework, but is independent of other Features (they communicate via EventBus).

---

## Architecture Overview

Gameplay is organized by **feature domain**:

```
Gameplay/
├── Bootstrap/        → Game initialization & service setup
├── Game State/       → High-level game state management
├── Player/           → Player controller & mechanics
├── UI/               → Shared gameplay HUD and contextual prompts
├── Enemy/            → Enemy AI with Behaviour Trees
├── Dialogue/         → Dialogue system & NPC conversations
├── Menu/             → Main menu, settings, UI views
├── Inventory/        → Item management & UI
├── Door/             → Interactive door mechanics
├── Delivery/         → Delivery/objective system
├── Environment/      → Map/world layout
└── Props/            → Environmental objects
```

---

## Core Features

### 1. Bootstrap (`Gameplay/Bootstrap`)
**Game initialization**—registers all Gameplay services and instantiates Core systems.

**Key:** `GameBootstrapper.cs` runs before any scene loads via `[RuntimeInitializeOnLoadMethod]`

See: [Bootstrap/README.md](./Bootstrap/README.md)

---

### 2. Game State (`Gameplay/Game State`)
**State machine**—manages high-level game flow (Gameplay, Menu, Paused, Dialogue, Cutscene).

**Key:** `GameStateService` responds to events and transitions between states.

See: [Game State/README.md](./Game%20State/README.md)

---

### 3. Player (`Gameplay/Player`)
**Player controller**—movement, camera, interaction, flashlight, and UI.

**Architecture:**
- `PlayerController.cs`: Main orchestrator
- `Components/`: Modular systems (Movement, Camera, Interaction, Flashlight, Jump)
- Movement uses a **state machine** (Walking, Sprinting, Crouching)

See: [Player/README.md](./Player/README.md)

---

### 4. Enemy (`Gameplay/Enemy`)
**Enemy AI**—uses Behaviour Trees for intelligent behavior.

**Architecture:**
- `EnemyController.cs`: Base controller (handles NavMesh, animation, events)
- `Scripts/Actions/`: Custom behaviour tree nodes (Chase, Patrol, Attack, Jumpscare)
- `Scripts/Enemies/`: Specific enemy types (LightSeeker, etc.)

See: [Enemy/README.md](./Enemy/README.md)

---

### 5. Dialogue (`Gameplay/Dialogue`)
**Dialogue system**—conversation playback, character data, line-by-line display.

**Architecture:**
- `DialogueService.cs`: Manages active dialogue, UI display, event flow
- `ScriptableObjects/`: `DialogueSO`, `CharacterSO` for dialogue/character data
- `Data/`: Dialogue and character asset files

See: [Dialogue/README.md](./Dialogue/README.md)

---

### 6. Menu (`Gameplay/Menu`)
**Menu system**—main menu, settings, pause menu, credits.

**Architecture:**
- `MenuService.cs`: Navigation and screen management
- `View/`: Individual menu screens (MainMenuView, SettingsMenuView, etc.)
- `Services/`: GameplayService, GraphicsService, ScreenService (Menu-specific implementations)
- `Default/`: Default settings for Audio, Graphics, Screen, Gameplay

See: [Menu/README.md](./Menu/README.md)

---

### 7. Inventory (`Gameplay/Inventory`)
**Inventory system**—item storage, equipment, UI display.

**Architecture:**
- `InventoryService.cs`: Manages items and capacity
- `InventoryUI.cs`: Displays inventory UI
- `Items/`: Item classes and data

See: [Inventory/README.md](./Inventory/README.md)

---

### 8. Door (`Gameplay/Door`)
**Door mechanics**—interactive doors that open/close with animation.

**Architecture:**
- `Door.cs`: Door controller (rotation, interaction, outline)
- `Door/` & `Front Door/`: Asset folders with models and materials

See: [Door/README.md](./Door/README.md)

---

### 9. Dialogue (`Gameplay/Delivery`)
**Delivery system**—objectives and delivery points.

**Key:** `DeliveryPoint.cs` represents a location where items can be delivered.

See: [Delivery/README.md](./Delivery/README.md)

---

### 10. Environment (`Gameplay/Environment`)
**World layout**—map/environment prefabs and models.

See: [Environment/README.md](./Environment/README.md)

---

### 11. Props (`Gameplay/Props`)
**Environmental objects**—tables, chairs, decorative elements.

---

## Feature Communication

Features communicate through **EventBus** (no direct coupling):

```
Player triggers Jump
    ↓
PlayerJump publishes JumpInputEvent via EventBus
    ↓
Multiple systems listen and respond:
    ├─ Audio: Plays jump sound
    ├─ Animation: Plays jump animation
    └─ Menu: Updates UI jump counter (if in UI mode)
```

---

## Adding a New Feature

### Step 1: Create Feature Folder
```
Gameplay/NewFeature/
├── README.md          (document the feature)
├── Scripts/           (code)
├── Prefabs/           (reusable objects)
├── Animations/        (animation files)
├── Audio/             (sound effects)
└── Data/              (ScriptableObjects)
```

### Step 2: Implement Core Service (if needed)

```csharp
public class NewFeatureService : MonoBehaviour, INewFeatureService {
    private IEventBus _eventBus;
    
    private void Start() {
        ServiceLocator.Register<INewFeatureService>(this);
        _eventBus = ServiceLocator.Get<IEventBus>();
        _eventBus.Subscribe<RelevantEvent>(OnRelevantEvent);
    }
}
```

### Step 3: Register in GameBootstrapper

```csharp
// In GameBootstrapper.Initialize()
var newFeatureService = new NewFeatureService();
ServiceLocator.Register<INewFeatureService>(newFeatureService);
```

### Step 4: Publish Events for Communication

```csharp
// When something important happens
var evt = new NewFeatureUpdatedEvent { Data = value };
_eventBus.Publish<NewFeatureUpdatedEvent>(evt);
```

---

## Best Practices

### 1. Keep Features Independent

❌ **Bad:**
```csharp
// Player depends directly on Enemy
public class Player : MonoBehaviour {
    private Enemy _targetEnemy;
}
```

✅ **Good:**
```csharp
// Player publishes events, Enemy listens
public class Player : MonoBehaviour {
    private void Attack() {
        _eventBus.Publish<PlayerAttackedEvent>(new(...));
    }
}

public class Enemy : MonoBehaviour {
    private void OnPlayerAttacked(PlayerAttackedEvent evt) {
        TakeDamage(evt.Damage);
    }
}
```

### 2. Use ScriptableObjects for Game Data

✅ **Good:**
```csharp
// Character data stored as ScriptableObject
[SerializeField] private CharacterSO _characterData;

// Can be easily edited in Inspector or loaded from Resources
var character = Resources.Load<CharacterSO>("Characters/Player");
```

### 3. Organize by Domain

✅ **Good:**
```
Enemy/
├── Scripts/
│   ├── EnemyController.cs     (base logic)
│   ├── Actions/               (behaviour tree nodes)
│   └── Enemies/               (specific types)
├── Prefabs/
├── Animations/
└── README.md
```

### 4. Document Each Feature

✅ **Good:**
```
Every feature folder has a README.md explaining:
- What the feature does
- How to use it
- Key components
- Examples
```

### 5. Respect State Transitions

✅ **Good:**
```csharp
// State transitions happen through events, not direct calls
_eventBus.Publish<GameStateChangedEvent>(new(GameState.Gameplay, GameState.Paused));

// NOT: gameState.CurrentState = GameState.Paused;
```

---

## Common Patterns

### Loading Feature Data

```csharp
// Load dialogue from Resources
var dialogue = Resources.Load<DialogueSO>("Dialogue/Intro");

// Load inventory items
var item = Resources.Load<Item>("Items/Flashlight");
```

### Responding to Events

```csharp
private void Start() {
    _eventBus.Subscribe<PlayerDiedEvent>(OnPlayerDied);
}

private void OnPlayerDied(PlayerDiedEvent evt) {
    ShowGameOverScreen();
}

private void OnDestroy() {
    _eventBus.Unsubscribe<PlayerDiedEvent>(OnPlayerDied);
}
```

### Triggering Game Events

```csharp
public void CompleteObjective() {
    _eventBus.Publish<ObjectiveCompletedEvent>(new(objectiveId));
}
```

---

## Architecture Flow

```
Game Starts
    ↓
GameBootstrapper.Initialize() [RuntimeInitializeOnLoadMethod]
    ├─ Clear ServiceLocator
    ├─ Create EventBus
    ├─ Create InputService
    ├─ Create InventoryService
    ├─ Create SettingsService
    ├─ Instantiate Core Systems prefab
    └─ Create MenuService
    ↓
Scene Loads
    ├─ GameStateService initializes (registers self)
    ├─ Player initializes
    ├─ Enemies initialize
    ├─ UI initializes
    └─ All features subscribe to relevant events
    ↓
Game Loop
    ├─ Input detected
    ├─ Events published
    ├─ Systems respond
    └─ Repeat
```

---

## Summary

The **Gameplay** layer provides:
- **Feature-based organization**: Each mechanic is isolated and self-contained
- **Event-driven communication**: Features talk via EventBus (no coupling)
- **ServiceLocator access**: All services available globally
- **Modular design**: Easy to add, remove, or modify features
- **Clear documentation**: Each feature has a README

By organizing Gameplay into independent features, PHOTOSSYNC achieves:
- **Scalability**: Add new features without touching existing code
- **Maintainability**: Each feature is easy to understand and debug
- **Reusability**: Features can be reused in other projects
- **Testability**: Services can be mocked for unit tests

**See also:**
- [Core/README.md](../Core/README.md) - Global services
- [Framework/README.md](../Framework/README.md) - Reusable systems
- Individual feature READMEs for implementation details
