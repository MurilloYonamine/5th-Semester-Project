# The `Door` Directory: Interactive Door Mechanics

The **Door** directory implements **interactive doors**—objects that open/close with animation, visual feedback (outline), and interaction support.

---

## Purpose

Door System provides:
- **Interactive door mechanics**: Open/close on player interaction
- **Smooth animation**: Lerp-based rotation between closed and open states
- **Visual feedback**: Outline highlights when looking at door
- **Interactable protocol**: Implements `IInteractable` interface
- **Mission dialogue adapter**: Optional deferred completion flow for mission-driven doors

---

## Key File

### `Door.cs`

Main component managing door state and animation.

```csharp
public class Door : MonoBehaviour {
    [Header("Visual")]
    [SerializeField] private Outline _outline;
    [SerializeField] private Transform _doorMesh;
    [SerializeField] private GameObject _textLocal;  // "Press E to open"

    [Header("Movement")]
    [SerializeField] private bool _isOpen = false;
    [SerializeField] private float _openAngle = 90f;   // Rotation amount
    [SerializeField] private float _speed = 5f;        // Animation speed

    private Quaternion _closedRotation;
    private Quaternion _targetRotation;
    public bool IsInteractable { get; private set; } = true;

    private void Awake() {
        _closedRotation = _doorMesh.localRotation;
        _targetRotation = _closedRotation;
    }

    private void Update() {
        // Smoothly rotate toward target
        _doorMesh.localRotation = Quaternion.Lerp(
            _doorMesh.localRotation,
            _targetRotation,
            Time.deltaTime * _speed
        );
    }

    public void Interact() {
        _isOpen = !_isOpen;

        if (_isOpen) {
            _targetRotation = _closedRotation * Quaternion.Euler(0, _openAngle, 0);
        } else {
            _targetRotation = _closedRotation;
        }
    }

    public void EnableOutline(bool enable) {
        if (_outline != null)
            _outline.enabled = enable;

        if (_textLocal != null)
            _textLocal.SetActive(enable);
    }
}
```

### `DoorMissionInteractionAdapter.cs`

Componente opcional no mesmo GameObject da porta para cenários de missão em que a interação deve:
- iniciar diálogo por `IDialogueService<TextAsset>`;
- adiar a conclusão da interação até o fim do diálogo;
- manter `PlayerInteraction` como ponto único de publicação de `ObjectSuccessfullyInteractedEvent`.

Fluxo:
1. Jogador interage com a porta.
2. `Door` delega para o adapter quando configurado.
3. Adapter inicia diálogo e marca conclusão adiada.
4. No `DialogueEndedEvent`, `PlayerInteraction` valida o adapter e só então publica a interação concluída.

---

## Component Breakdown

### Door Mesh
- Transform containing the door's 3D model
- Rotates when opening/closing
- Should be a child of the Door GameObject

### Outline
- Visual feedback when player looks at door
- Enabled/disabled by `EnableOutline()`
- From QuickOutline asset

### Text Display
- Shows "Press E to interact" hint
- Only visible when player is looking at door

---

## Setup

### Creating a Door Prefab

1. Create a GameObject: `Door_Bedroom`
2. Add `Door.cs` component
3. Create child: `DoorMesh` (import FBX or cube)
4. Add `Outline` component to DoorMesh (from QuickOutline)
5. Create child: `InteractionText` (TextMeshProUGUI)
6. Configure in Inspector:
   - Drag DoorMesh into Door Mesh field
   - Drag outline component into Outline field
   - Drag text into Text Local field
   - Set Open Angle (90° for hinged door)
   - Set Speed (5 is smooth)
   - Drag prefab into Prefabs/ folder

---

## Usage

### Detecting Player Interaction

```csharp
// In PlayerInteraction.cs
private void DetectInteractable() {
    if (Physics.Raycast(_camera.transform.position, _camera.transform.forward, out RaycastHit hit)) {
        IInteractable interactable = hit.collider.GetComponent<IInteractable>();
        
        if (interactable != null) {
            // Show interaction hint
            _currentInteractable = interactable;
            
            if (Input.GetKeyDown(KeyCode.E)) {
                _currentInteractable.Interact();
            }
        }
    }
}
```

### Looking at Door

```csharp
// When raycast hits door
Door door = hit.collider.GetComponent<Door>();
if (door != null) {
    door.EnableOutline(true);   // Highlight door
}
```

### Door Opens on Interaction

```csharp
// Player presses E
door.Interact();  // Door toggles between open/closed

// Smoothly animates over frames
// Door.Update() handles lerp
```

---

## Animation Details

### Closed Position
```csharp
_closedRotation = _doorMesh.localRotation;  // Initial rotation
```

### Open Position
```csharp
// Rotate from closed position by openAngle degrees
_targetRotation = _closedRotation * Quaternion.Euler(0, _openAngle, 0);
```

### Smooth Lerp
```csharp
// Each frame, rotate toward target at speed
_doorMesh.localRotation = Quaternion.Lerp(
    currentRotation,
    targetRotation,
    Time.deltaTime * speed
);
```

**Result:** Door smoothly opens/closes over ~0.5 seconds

---

## Prefab Structure

```
Door (GameObject)
├── Door.cs (component)
├── Mesh Collider (for raycast)
└── DoorMesh (child)
    ├── SkinnedMeshRenderer (visual)
    ├── Outline (highlights when looking)
    └── (FBX model data)
└── InteractionText (child)
    └── TextMeshProUGUI ("Press E to open")
```

---

## Configuration

### Opening Angle
```csharp
[SerializeField] private float _openAngle = 90f;
```
- 90° for standard hinged door
- Adjust for sliding doors or double doors

### Animation Speed
```csharp
[SerializeField] private float _speed = 5f;
```
- Higher = faster open/close
- Lower = more dramatic slow animation

### Outline Visibility
```csharp
public void EnableOutline(bool enable) {
    _outline.enabled = enable;      // Visual highlight
    _textLocal.SetActive(enable);   // Show "Press E" hint
}
```

---

## Best Practices

### 1. Position Door Correctly

✅ **Good:**
```
Door is at (0, 0, 0)
DoorMesh is child at local (0, 0, 0)
Rotates around its own pivot (door frame hinge)
```

❌ **Bad:**
```
Door at center of room
Rotates around center of room instead of frame
```

### 2. Use Local Rotation

✅ **Good:**
```csharp
// Rotate relative to door's local frame
_doorMesh.localRotation = Quaternion.Lerp(
    _doorMesh.localRotation,
    _targetRotation,
    Time.deltaTime * _speed
);
```

### 3. Set Hinge Point

✅ **Good:**
```
DoorMesh should have its pivot at the hinge
Import FBX with correct pivot positioning
```

### 4. Lock During Animation (Optional)

```csharp
public bool IsMoving => Vector3.Distance(
    _doorMesh.localRotation.eulerAngles,
    _targetRotation.eulerAngles
) > 0.1f;

public void Interact() {
    if (IsMoving) return;  // Can't open while closing
    
    _isOpen = !_isOpen;
    // ...
}
```

### 5. Sound Effect on Interaction

```csharp
public void Interact() {
    _isOpen = !_isOpen;
    
    // Play door creak sound
    var audio = ServiceLocator.Get<IAudioService>();
    audio.PlaySFX("Doors/Creak", volume: 0.8f);
    
    // ...
}
```

---

## Extending Doors

### Locked Door

```csharp
[SerializeField] private bool _isLocked = false;

public void Interact() {
    if (_isLocked) {
        Debug.Log("Door is locked");
        return;
    }
    
    _isOpen = !_isOpen;
    // ...
}
```

### Door with Key

```csharp
[SerializeField] private int _requiredKeyId = 1;

public void Unlock(int keyId) {
    if (keyId == _requiredKeyId) {
        _isLocked = false;
    }
}
```

### Sliding Door

```csharp
// Instead of rotating, translate along axis
private void Update() {
    Vector3 targetPos = _isOpen 
        ? _closedPosition + Vector3.right * _openDistance
        : _closedPosition;
    
    _doorMesh.localPosition = Vector3.Lerp(
        _doorMesh.localPosition,
        targetPos,
        Time.deltaTime * _speed
    );
}
```

### Auto-Closing Door

```csharp
private float _openTimer = 0;
private const float AUTO_CLOSE_TIME = 5f;

public void Interact() {
    _isOpen = !_isOpen;
    _openTimer = 0;
}

private void Update() {
    if (_isOpen) {
        _openTimer += Time.deltaTime;
        
        if (_openTimer >= AUTO_CLOSE_TIME) {
            _isOpen = false;
        }
    }
    
    // Animate door...
}
```

---

## Prefab Variants

### Internal Door (`Door/`)
- Standard hinged internal door
- 90° rotation
- Normal speed

### Front Door (`Front Door/`)
- Larger, more ornate
- Might be glass/metal
- Could be locked

Create as separate prefab variants in Prefabs/ folder.

---

## Debugging

### Visualize Hinge Point

```csharp
private void OnDrawGizmos() {
    // Show where door rotates from
    Gizmos.color = Color.red;
    Gizmos.DrawSphere(_doorMesh.position, 0.1f);
}
```

### Log State

```csharp
public void Interact() {
    _isOpen = !_isOpen;
    Debug.Log($"Door is now {(_isOpen ? "OPEN" : "CLOSED")}");
}
```

---

## Summary

The **Door** system provides:
- **Interactive mechanics**: Open/close on interaction
- **Smooth animation**: Lerp-based rotation
- **Visual feedback**: Outlines and hints
- **Simple architecture**: Minimal dependencies
- **Extensible**: Easy to add locks, sounds, variants

By using Door component:
- **Designers** can place doors easily
- **Players** have clear interaction feedback
- **Doors** behave consistently
- **World feels interactive**

**See also:**
- [Gameplay/README.md](../README.md) - Gameplay features
- IInteractable interface for other interactive objects
