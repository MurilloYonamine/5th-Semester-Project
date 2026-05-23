# The `Player` Directory: Player Controller & Mechanics

The **Player** directory implements the **player character**—movement with state machine (walking/sprinting/crouching), camera control, interaction system, flashlight, and UI updates.

---

## Purpose

Player System provides:
- **State-based movement**: Walking, sprinting, crouching with smooth transitions
- **First-person camera**: Smooth look around
- **Interaction system**: Cast rays to detect and interact with objects
- **Flashlight**: Toggle-able light with energy management
- **Event communication**: Notify other systems of player actions

---

## Architecture

```
PlayerController.cs (orchestrator)
├── Components/
│   ├── PlayerMovement.cs (walking/sprinting/crouching)
│   ├── PlayerCamera.cs (first-person view)
│   ├── PlayerInteraction.cs (raycasting for interactables)
│   ├── PlayerFlashlight.cs (light and energy)
│   └── PlayerUI.cs (health, stamina display)
├── State Machine
│   ├── Idle
│   ├── Walking
│   ├── Sprinting
│   └── Crouching
└── Event Publishing
    ├── PlayerSprintChangedEvent
    ├── PlayerDamagedEvent
    └── PlayerInteractedEvent
```

---

## Key Files

### `PlayerController.cs`

Main orchestrator that initializes all player systems.

```csharp
public class PlayerController : MonoBehaviour {
    public static PlayerController Instance { get; private set; }

    [SerializeField] private PlayerMovement _movement;
    [SerializeField] private PlayerCamera _camera;
    [SerializeField] private PlayerInteraction _interaction;
    [SerializeField] private PlayerFlashlight _flashlight;
    [SerializeField] private PlayerUI _ui;

    [Header("Stats")]
    [SerializeField] private float _maxHealth = 100f;
    [SerializeField] private float _maxStamina = 100f;

    private float _currentHealth;
    private float _currentStamina;
    private IEventBus _eventBus;

    private void Awake() {
        Instance = this;
    }

    private void Start() {
        _currentHealth = _maxHealth;
        _currentStamina = _maxStamina;
        _eventBus = ServiceLocator.Get<IEventBus>();

        // Enable all components
        _movement.Initialize(_eventBus);
        _camera.Initialize();
        _interaction.Initialize(_eventBus);
        _flashlight.Initialize(_eventBus);
        _ui.Initialize(_currentHealth, _currentStamina);
    }

    private void Update() {
        _movement.Update();
        _interaction.Update();
        _camera.Update();
        _ui.UpdateDisplay(_currentHealth, _currentStamina);
    }

    public void TakeDamage(float amount) {
        _currentHealth -= amount;
        _eventBus.Publish<PlayerDamagedEvent>(new(amount, _currentHealth));

        if (_currentHealth <= 0) {
            Die();
        }
    }

    private void Die() {
        _eventBus.Publish<PlayerDiedEvent>(new());
        enabled = false;
    }
}
```

---

### `Components/PlayerMovement.cs`

Handles walking, sprinting, crouching with state machine.

```csharp
public class PlayerMovement : MonoBehaviour {
    [Header("Movement")]
    [SerializeField] private float _walkSpeed = 5f;
    [SerializeField] private float _sprintSpeed = 8f;
    [SerializeField] private float _crouchSpeed = 2.5f;
    [SerializeField] private float _staminaDrainRate = 10f;
    [SerializeField] private float _staminaRecoverRate = 5f;

    private CharacterController _controller;
    private float _currentSpeed;
    private float _verticalVelocity = 0;
    private IEventBus _eventBus;
    private IInputService _inputService;

    private enum MovementState {
        Walking,
        Sprinting,
        Crouching
    }
    private MovementState _state = MovementState.Walking;

    public void Initialize(IEventBus eventBus) {
        _eventBus = eventBus;
        _inputService = ServiceLocator.Get<IInputService>();
        _controller = GetComponent<CharacterController>();
    }

    public void Update() {
        // Get input
        Vector2 moveInput = _inputService.GetMovementInput();
        bool sprintPressed = _inputService.IsSprintPressed();
        bool crouchPressed = _inputService.IsCrouchPressed();

        // State machine
        UpdateMovementState(moveInput, sprintPressed, crouchPressed);

        // Apply gravity
        _verticalVelocity -= Physics.gravity.y * Time.deltaTime;

        // Move character
        Vector3 movement = new Vector3(moveInput.x, _verticalVelocity, moveInput.y) * _currentSpeed * Time.deltaTime;
        _controller.Move(movement);
    }

    private void UpdateMovementState(Vector2 moveInput, bool sprintPressed, bool crouchPressed) {
        bool hasMovementInput = moveInput.magnitude > 0;

        if (crouchPressed) {
            SetState(MovementState.Crouching);
        } else if (sprintPressed && hasMovementInput) {
            SetState(MovementState.Sprinting);
        } else {
            SetState(MovementState.Walking);
        }
    }

    private void SetState(MovementState newState) {
        if (_state == newState) return;

        _state = newState;
        _currentSpeed = _state switch {
            MovementState.Walking => _walkSpeed,
            MovementState.Sprinting => _sprintSpeed,
            MovementState.Crouching => _crouchSpeed,
            _ => _walkSpeed
        };

        // Publish event for enemies to react
        if (newState == MovementState.Sprinting) {
            _eventBus.Publish<PlayerSprintChangedEvent>(new(true));
        } else {
            _eventBus.Publish<PlayerSprintChangedEvent>(new(false));
        }
    }
}
```

**State Transitions:**
```
Walking ←→ Sprinting (if moving + Sprint key)
   ↓
Crouching (overrides both)
```

---

### `Components/PlayerCamera.cs`

First-person camera with mouse/gamepad look.

```csharp
public class PlayerCamera : MonoBehaviour {
    [SerializeField] private Transform _head;
    [SerializeField] private float _mouseSensitivity = 2f;
    [SerializeField] private float _minVerticalAngle = -90f;
    [SerializeField] private float _maxVerticalAngle = 90f;

    private float _horizontalAngle = 0;
    private float _verticalAngle = 0;
    private IInputService _inputService;

    public void Initialize() {
        _inputService = ServiceLocator.Get<IInputService>();
    }

    public void Update() {
        Vector2 lookInput = _inputService.GetLookInput();

        _horizontalAngle += lookInput.x * _mouseSensitivity;
        _verticalAngle -= lookInput.y * _mouseSensitivity;
        _verticalAngle = Mathf.Clamp(_verticalAngle, _minVerticalAngle, _maxVerticalAngle);

        // Apply rotation
        transform.rotation = Quaternion.Euler(0, _horizontalAngle, 0);
        _head.localRotation = Quaternion.Euler(_verticalAngle, 0, 0);
    }
}
```

**Result:** Smooth first-person camera with clamped vertical look

---

### `Components/PlayerInteraction.cs`

Detects and triggers interactions with objects.

```csharp
public class PlayerInteraction : MonoBehaviour {
    [SerializeField] private float _interactionDistance = 3f;
    [SerializeField] private int _interactionLayer;

    private Camera _camera;
    private IInteractable _currentInteractable;
    private IInputService _inputService;
    private IEventBus _eventBus;

    public void Initialize(IEventBus eventBus) {
        _eventBus = eventBus;
        _inputService = ServiceLocator.Get<IInputService>();
        _camera = GetComponent<Camera>();
    }

    public void Update() {
        DetectInteractable();

        if (_inputService.IsInteractPressed()) {
            TryInteract();
        }
    }

    private void DetectInteractable() {
        Ray ray = _camera.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, _interactionDistance)) {
            IInteractable interactable = hit.collider.GetComponent<IInteractable>();

            if (interactable != null && interactable != _currentInteractable) {
                _currentInteractable?.OnLookAway();
                _currentInteractable = interactable;
                _currentInteractable.OnLookAt();  // Show hint
            }
        } else {
            _currentInteractable?.OnLookAway();
            _currentInteractable = null;
        }
    }

    private void TryInteract() {
        if (_currentInteractable != null && _currentInteractable.IsInteractable) {
            _currentInteractable.Interact();
            _eventBus.Publish<PlayerInteractedEvent>(new());
        }
    }
}
```

**Logic:**
```
Each frame:
  Cast ray from camera center
  If hit IInteractable:
    Show interaction hint
  Else:
    Hide hint
  
Player presses E:
  Call Interact() on current interactable
```

---

### `Components/PlayerFlashlight.cs`

Toggleable light with energy drain/recovery.

```csharp
public class PlayerFlashlight : MonoBehaviour {
    [SerializeField] private Light _light;
    [SerializeField] private float _maxEnergy = 100f;
    [SerializeField] private float _drainRate = 20f;
    [SerializeField] private float _recoveryRate = 10f;

    private float _currentEnergy;
    private bool _isOn = false;
    private IEventBus _eventBus;
    private IInputService _inputService;

    public void Initialize(IEventBus eventBus) {
        _eventBus = eventBus;
        _inputService = ServiceLocator.Get<IInputService>();
        _currentEnergy = _maxEnergy;
        _light.enabled = false;
    }

    public void Update() {
        if (_inputService.IsFlashlightToggled()) {
            ToggleFlashlight();
        }

        // Energy management
        if (_isOn) {
            _currentEnergy -= _drainRate * Time.deltaTime;
            
            if (_currentEnergy <= 0) {
                ToggleFlashlight();  // Auto-off when depleted
            }
        } else {
            _currentEnergy = Mathf.Min(_currentEnergy + _recoveryRate * Time.deltaTime, _maxEnergy);
        }

        _light.intensity = _isOn ? 1f : 0f;
    }

    private void ToggleFlashlight() {
        _isOn = !_isOn;
        _light.enabled = _isOn;
        _eventBus.Publish<FlashlightToggleEvent>(new(_isOn));
    }

    public float GetEnergyPercent() => _currentEnergy / _maxEnergy;
}
```

---

### `Components/PlayerUI.cs`

Updates HUD with health, stamina, energy.

```csharp
public class PlayerUI : MonoBehaviour {
    [SerializeField] private Slider _healthBar;
    [SerializeField] private Slider _staminaBar;
    [SerializeField] private Slider _flashlightBar;
    [SerializeField] private TextMeshProUGUI _healthText;

    private IEventBus _eventBus;
    private PlayerFlashlight _flashlight;

    public void Initialize(float maxHealth, float maxStamina) {
        _eventBus = ServiceLocator.Get<IEventBus>();
        _flashlight = GetComponent<PlayerFlashlight>();
        
        _healthBar.maxValue = maxHealth;
        _staminaBar.maxValue = maxStamina;
        _flashlightBar.maxValue = 100f;
    }

    public void UpdateDisplay(float health, float stamina) {
        _healthBar.value = health;
        _healthText.text = $"{Mathf.RoundToInt(health)} / {Mathf.RoundToInt(_healthBar.maxValue)}";
        
        _staminaBar.value = stamina;
        
        _flashlightBar.value = _flashlight.GetEnergyPercent() * 100f;
    }
}
```

---

### `Scripts/UI/PlayerStateUIController.cs`

Controls player HUD visibility based on the current `GameState`.

```csharp
public class PlayerStateUIController : MonoBehaviour {
    [SerializeField] private GameObject _crosshair;
    [SerializeField] private GameObject _staminaBarRoot;
}
```

Use it to hide the crosshair and stamina bar whenever the game is not in `Gameplay`.

---

## Setup

### Player Prefab

```
Player (GameObject)
├── PlayerController.cs
├── CharacterController (physics)
├── Head (child)
│   └── Main Camera
│       ├── PlayerCamera.cs
│       └── PlayerInteraction.cs
├── PlayerMovement.cs
├── PlayerFlashlight.cs
│   └── Spotlight (child)
│       └── Light component
└── Canvas (UI)
    └── PlayerUI.cs
```

### Configuration

In Inspector:
```
Walk Speed: 5
Sprint Speed: 8
Crouch Speed: 2.5
Max Stamina: 100
Max Health: 100
```

---

## Usage

### Moving

```
WASD: Move
Shift: Sprint (while moving)
Ctrl: Crouch
```

### Looking

```
Mouse: Look around
```

### Interaction

```
E: Interact with highlighted object
F: Toggle flashlight
```

---

## Best Practices

### 1. Separate Concerns

✅ **Good:**
```csharp
// PlayerMovement handles only movement
public class PlayerMovement : MonoBehaviour {
    public void Update() {
        // Only movement logic
    }
}

// PlayerCamera handles only camera
public class PlayerCamera : MonoBehaviour {
    public void Update() {
        // Only camera logic
    }
}
```

### 2. Use Events for Communication

✅ **Good:**
```csharp
// PlayerMovement publishes event
_eventBus.Publish<PlayerSprintChangedEvent>(new(true));

// Enemies listen
private void OnPlayerSprint(PlayerSprintChangedEvent evt) {
    IncreaseAggression();
}
```

### 3. Cache InputService

✅ **Good:**
```csharp
private IInputService _inputService;

public void Initialize() {
    _inputService = ServiceLocator.Get<IInputService>();
}

// Use cached reference
Vector2 input = _inputService.GetMovementInput();
```

### 4. Clamp Camera Angles

✅ **Good:**
```csharp
_verticalAngle = Mathf.Clamp(_verticalAngle, -90f, 90f);
// Prevents camera flipping
```

### 5. Raycast for Interactions

✅ **Good:**
```csharp
Ray ray = _camera.ScreenPointToRay(new Vector3(Screen.width/2, Screen.height/2));
Physics.Raycast(ray, out RaycastHit hit, 3f);
// Center of screen, 3 units range
```

---

## Common Patterns

### Health System

```csharp
public void TakeDamage(float amount) {
    _currentHealth -= amount;
    _eventBus.Publish<PlayerDamagedEvent>(new(amount));
    
    if (_currentHealth <= 0) {
        Die();
    }
}

private void Die() {
    _eventBus.Publish<PlayerDiedEvent>(new());
    enabled = false;
}
```

### Stamina Management

```csharp
private void UpdateMovementState() {
    if (sprintPressed && _currentStamina > 0) {
        _currentStamina -= _staminaDrainRate * Time.deltaTime;
        SetState(MovementState.Sprinting);
    } else {
        _currentStamina = Mathf.Min(_currentStamina + _staminaRecoverRate * Time.deltaTime, _maxStamina);
    }
}
```

### Interaction Flow

```
Player aims at object
  ↓
Raycast detects IInteractable
  ↓
Call OnLookAt() (show hint)
  ↓
Player presses E
  ↓
Call Interact() (perform action)
  ↓
OnLookAway() (hide hint)
```

---

## Extending Player

### Jumping

```csharp
public class PlayerMovement : MonoBehaviour {
    private float _jumpForce = 5f;

    private void Update() {
        if (_inputService.IsJumpPressed() && IsGrounded()) {
            _verticalVelocity = _jumpForce;
        }
    }

    private bool IsGrounded() {
        return Physics.Raycast(transform.position, Vector3.down, 0.1f);
    }
}
```

### Inventory Interaction

```csharp
public class PlayerInteraction : MonoBehaviour {
    private void TryInteract() {
        if (_currentInteractable != null) {
            // Check if it's a pickup
            if (_currentInteractable is IPickup pickup) {
                var inventory = ServiceLocator.Get<IInventoryService<Item>>();
                if (inventory.TryAdd(pickup.GetItem())) {
                    pickup.OnPickedUp();
                }
            } else {
                _currentInteractable.Interact();
            }
        }
    }
}
```

---

## Debugging

### Print Current State

```csharp
private void OnGUI() {
    GUI.Label(new Rect(10, 10, 200, 20), 
        $"State: {_state} | Health: {_currentHealth}");
}
```

### Visualize Interaction Ray

```csharp
private void OnDrawGizmos() {
    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
    Gizmos.color = Color.red;
    Gizmos.DrawRay(ray.origin, ray.direction * 3f);
}
```

---

## Summary

The **Player** system provides:
- **State-based movement**: Walking, sprinting, crouching
- **First-person camera**: Smooth, clamped look
- **Interaction system**: Raycast-based object detection
- **Flashlight mechanics**: Energy management
- **Event-driven**: Notifies other systems of player actions

By using modular components:
- **Easy to maintain**: Each component has one job
- **Easy to extend**: Add jumping, abilities, etc.
- **Easy to debug**: Debug each component separately
- **Events decouple**: Player doesn't know about enemies

**See also:**
- [Gameplay/README.md](../README.md) - Gameplay features
- [Core/Input/README.md](../../Core/Input/README.md) - Input system
