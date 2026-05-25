# The `Shared` Directory: Common Interfaces & Utilities

The **Shared** directory contains **cross-project utilities and interfaces**—reusable components, physical materials, and interaction patterns used throughout all features.

---

## Purpose

Shared System provides:
- **Interaction interface**: Standard contract for interactive objects
- **Screen utilities**: Aspect ratio management for PSX aesthetic
- **Movement utilities**: Floating/oscillating animations
- **Physics materials**: Shared physical properties
- **Common patterns**: Standardized behaviors across features

---

## Directory Structure

```
Shared/
├── README.md              (this file)
├── Scripts/
│   ├── IInteractable.cs   (interaction interface)
│   ├── IDeferredInteractionCompletion.cs (interaction completion strategy)
│   ├── AspectRatioController.cs
│   ├── Floating.cs        (floating animation)
│   └── [other utilities]
├── No Friction.physicMaterial
└── FifthSemester.Shared.asmdef (assembly definition)
```

---

## Key Files

### `IInteractable.cs`

Interface for all interactive objects in the game.

```csharp
namespace FifthSemester.Gameplay.Shared {
    public interface IInteractable {
        string Id { get; }                    // Unique identifier
        bool IsInteractable { get; }          // Can be interacted with?
        
        void Interact();                      // Perform interaction
        void StopInteract();                  // Stop interaction
        void Highlight(bool value);           // Show/hide interaction hint
    }
}
```

#### Implementation Example

```csharp
public class Door : MonoBehaviour, IInteractable {
    [SerializeField] private string _id = "door_bedroom";
    [SerializeField] private Outline _outline;
    
    public string Id => _id;
    public bool IsInteractable => !_isOpen;

    public void Interact() {
        _isOpen = !_isOpen;
        // Animate door...
    }

    public void StopInteract() {
        // Cancel interaction if in progress
    }

    public void Highlight(bool value) {
        _outline.enabled = value;
    }
}
```

#### Usage

```csharp
// In PlayerInteraction.cs
if (Physics.Raycast(ray, out RaycastHit hit, 3f)) {
    IInteractable interactable = hit.collider.GetComponent<IInteractable>();
    
    if (interactable != null) {
        interactable.Highlight(true);  // Show hint
        
        if (Input.GetKeyDown(KeyCode.E)) {
            interactable.Interact();    // Perform action
        }
    }
}
```

---

### `IDeferredInteractionCompletion.cs`

Interface opcional para interações que só devem concluir depois de um evento posterior (ex.: fim de diálogo).

```csharp
namespace FifthSemester.Gameplay.Shared {
    public interface IDeferredInteractionCompletion {
        bool PublishInteractionOnInput { get; }
        bool TryCompleteDeferredInteraction(string sourceId);
    }
}
```

Uso esperado:
- `PlayerInteraction` continua como ponto único de publicação de `ObjectSuccessfullyInteractedEvent`.
- Objetos com conclusão adiada retornam `PublishInteractionOnInput = false` no clique inicial.
- Quando o evento de confirmação ocorre (ex.: `DialogueEndedEvent`), `PlayerInteraction` consulta `TryCompleteDeferredInteraction` e publica a interação uma única vez.

---

### `AspectRatioController.cs`

Locks camera to 4:3 aspect ratio for retro PSX aesthetic.

```csharp
public class AspectRatioController : MonoBehaviour {
    private readonly float targetAspect = 4f / 3f;

    private void Start() {
        ApplyAspect();
    }

    private void ApplyAspect() {
        float windowAspect = (float)Screen.width / Screen.height;
        float scaleHeight = windowAspect / targetAspect;

        Camera cam = GetComponent<Camera>();
        cam.backgroundColor = Color.black;

        if (scaleHeight < 1.0f) {
            // Window too tall—add black bars on sides
            Rect rect = cam.rect;
            rect.width = 1.0f;
            rect.height = scaleHeight;
            rect.x = 0;
            rect.y = (1.0f - scaleHeight) / 2.0f;
            cam.rect = rect;
        }
        else {
            // Window too wide—add black bars top/bottom
            float scaleWidth = 1.0f / scaleHeight;

            Rect rect = cam.rect;
            rect.width = scaleWidth;
            rect.height = 1.0f;
            rect.x = (1.0f - scaleWidth) / 2.0f;
            rect.y = 0;
            cam.rect = rect;
        }
    }
}
```

#### How It Works

```
Player's Monitor: 16:9 (1920×1080)
    ↓
AspectRatioController: Force 4:3
    ├─ Calculate viewport size
    ├─ Add black letterbox bars
    └─ Render game at 4:3
    ↓
Result: Retro PSX look with black bars
```

#### Setup

1. Add script to Main Camera
2. Script runs on Start()
3. Automatically letterboxes based on screen resolution

---

### `Floating.cs`

Simple floating/bobbing animation using sine wave.

```csharp
public class Floating : MonoBehaviour {
    public float amplitude = 0.5f;  // Max distance up/down
    public float speed = 1f;        // Oscillation speed
    
    private float startY;
    private Vector3 tempPos;

    private void Start() {
        startY = transform.position.y;  // Store starting height
    }

    private void Update() {
        tempPos = transform.position;
        // Sine wave creates smooth up-down motion
        tempPos.y = startY + amplitude * Mathf.Sin(speed * Time.time);
        transform.position = tempPos;
    }
}
```

#### Motion Pattern

```
Position
    ↑
    │     ╱╲     ╱╲
    │    ╱  ╲   ╱  ╲
    │───┼────╲─╱────╲─
    │   │     ╲│
    └─────────────────→ Time
    
amplitude = distance from center
speed = oscillation rate
```

#### Usage

```csharp
// Attach to floating object (pickup item, collectible, etc.)
// Set in Inspector:
// - Amplitude: 0.5 (bob up/down 0.5 units)
// - Speed: 2 (fast bobbing)

// The component handles all movement automatically
// No code needed—set and forget
```

#### Common Settings

```
Slow, gentle bobbing:
  amplitude = 0.3
  speed = 0.5

Fast, energetic bouncing:
  amplitude = 0.8
  speed = 3

Subtle hovering:
  amplitude = 0.1
  speed = 1
```

---

### `No Friction.physicMaterial`

Physics material with zero friction for slippery surfaces.

```
Material Properties:
- Dynamic Friction: 0
- Static Friction: 0
- Bounciness: 0
- Friction Combine: Minimum
- Bounce Combine: Average
```

#### Usage

```csharp
// Apply to slippery surface colliders
Collider iceFloor = GetComponent<Collider>();
iceFloor.material = Resources.Load<PhysicMaterial>("No Friction");
```

#### When to Use

- Ice floors
- Slippery surfaces
- Frictionless movement areas
- Player slides without control

---

## Interaction Pattern

### Standard Interaction Flow

```
Player looks at object (raycast center screen)
    ↓
Object implements IInteractable? YES
    ├─ interactable.Highlight(true)
    │  (Show "Press E" hint)
    │
    └─ Player presses E
       ├─ interactable.Interact()
       │  (Perform action: open door, pick up item, etc.)
       └─ After action done
          └─ interactable.Highlight(false)
             (Hide hint)

Player looks away
    ↓
interactable.Highlight(false)
(Hide hint)
```

### Implementing IInteractable

```csharp
public class Item : MonoBehaviour, IInteractable {
    public string Id => "item_" + gameObject.name;
    public bool IsInteractable => !_collected;

    public void Interact() {
        var inventory = ServiceLocator.Get<IInventoryService<Item>>();
        if (inventory.TryAdd(this)) {
            _collected = true;
            Destroy(gameObject);
        }
    }

    public void StopInteract() {
        // Nothing to stop for pickup
    }

    public void Highlight(bool value) {
        GetComponent<Outline>().enabled = value;
    }
}
```

---

## Best Practices

### 1. Implement IInteractable Consistently

✅ **Good:**
```csharp
public class Door : MonoBehaviour, IInteractable {
    public string Id => "door_main";
    public bool IsInteractable => !_isLocked;
    
    public void Interact() { /* open door */ }
    public void StopInteract() { /* stop action */ }
    public void Highlight(bool value) { /* show hint */ }
}
```

### 2. Use Unique IDs

✅ **Good:**
```csharp
public string Id => "door_hospital_entrance";
// Unique, descriptive identifier
```

❌ **Bad:**
```csharp
public string Id => "door1";  // Not unique
public string Id => gameObject.name;  // Changes with rename
```

### 3. Respect IsInteractable Flag

✅ **Good:**
```csharp
public bool IsInteractable => !_used && !_locked;
// Only interact if conditions are met

public void Interact() {
    if (!IsInteractable) return;
    // ... perform action
}
```

### 4. Provide Visual Feedback

✅ **Good:**
```csharp
public void Highlight(bool value) {
    _outline.enabled = value;
    _hintText.gameObject.SetActive(value);
}
```

### 5. Use Floating for Collectibles

✅ **Good:**
```
// Attach Floating to pickup items
// Makes them visually distinctive
// Indicates "collectable" status
```

---

## Common Patterns

### Interactive Door

```csharp
public class InteractiveDoor : MonoBehaviour, IInteractable {
    [SerializeField] private string _doorId = "door_main";
    [SerializeField] private Outline _outline;
    private bool _isOpen = false;

    public string Id => _doorId;
    public bool IsInteractable => true;

    public void Interact() {
        _isOpen = !_isOpen;
        // Animate door...
    }

    public void StopInteract() {}

    public void Highlight(bool value) {
        _outline.enabled = value;
    }
}
```

### Collectible Item

```csharp
public class CollectibleItem : MonoBehaviour, IInteractable {
    [SerializeField] private Item _itemData;
    [SerializeField] private Floating _floating;

    public string Id => _itemData.id;
    public bool IsInteractable => !_collected;
    private bool _collected = false;

    public void Interact() {
        var inventory = ServiceLocator.Get<IInventoryService<Item>>();
        if (inventory.TryAdd(_itemData)) {
            _collected = true;
            Destroy(gameObject);
        }
    }

    public void StopInteract() {}

    public void Highlight(bool value) {
        // Visual feedback
    }
}
```

### Locked Container

```csharp
public class LockedContainer : MonoBehaviour, IInteractable {
    [SerializeField] private int _requiredKeyId;
    private bool _isOpen = false;

    public string Id => "container_" + gameObject.name;
    public bool IsInteractable => !_isOpen;

    public void Interact() {
        var inventory = ServiceLocator.Get<IInventoryService<Item>>();
        
        // Check for key
        if (HasKeyInInventory(inventory)) {
            _isOpen = true;
            GrantReward();
        } else {
            ShowLockedMessage();
        }
    }

    public void StopInteract() {}

    public void Highlight(bool value) {
        // Show lock icon
    }
}
```

---

## Extending Shared

### Custom Utility Component

```csharp
public class RotatingObject : MonoBehaviour {
    [SerializeField] private Vector3 _rotationAxis = Vector3.up;
    [SerializeField] private float _rotationSpeed = 45f;  // degrees/sec

    private void Update() {
        transform.Rotate(_rotationAxis * _rotationSpeed * Time.deltaTime);
    }
}
```

### Custom Physics Material

```csharp
// Create in Editor: Right-click → Create → Physics Material
// Set properties:
// - Dynamic Friction: 1.0
// - Static Friction: 1.0
// - Bounciness: 0.5

// Use:
collider.material = Resources.Load<PhysicMaterial>("BounceRubber");
```

### Enhanced Interaction Feedback

```csharp
public void Highlight(bool value) {
    // Visual
    _outline.enabled = value;
    
    // Audio
    if (value) {
        AudioSource.PlayClipAtPoint(_hoverSound, transform.position);
    }
    
    // Animation
    if (value) {
        _animator.SetTrigger("Highlight");
    }
}
```

---

## Assembly Definition

```
FifthSemester.Shared.asmdef

Dependencies:
- Unity.InputSystem (if using input in utilities)
- (minimal external dependencies for maximum reusability)
```

This assembly is referenced by:
- FifthSemester.Core
- FifthSemester.Framework
- FifthSemester.Gameplay
- All features

---

## Summary

The **Shared** directory provides:
- **IInteractable interface**: Standardized interaction pattern
- **Aspect ratio control**: PSX retro aesthetic (4:3 letterbox)
- **Floating animation**: Sine-wave bobbing motion
- **Physics materials**: Slippery surface handling
- **Common patterns**: Reusable interaction implementations

By using Shared utilities:
- **Consistency**: All interactive objects follow same pattern
- **Reusability**: Components work across all features
- **Maintainability**: Changes in one place affect all uses
- **Aesthetic**: Unified visual style (4:3 ratio)
- **Simplicity**: Easy to add common behaviors

**See also:**
- [Gameplay/Door/README.md](../Gameplay/Door/README.md) - Door uses IInteractable
- [Gameplay/Player/README.md](../Gameplay/Player/README.md) - Player interaction detection
- [Gameplay/Inventory/README.md](../Gameplay/Inventory/README.md) - Inventory items are IInteractable
