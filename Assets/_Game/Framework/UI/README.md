# The `UI` Directory: Reusable UI Components

The **UI** directory contains **generic, game-agnostic UI components** that can be reused across different features and even different projects. These components provide common interaction patterns without depending on specific game logic.

---

## Architecture Overview

The UI Framework is organized by component:

1. **`Selector/`**: Generic option selector widget (dropdown-style selection)

Each component typically contains:
- **Scripts/**: C# implementation
- **Prefabs/**: Pre-built, configured prefab for easy reuse

---

## 1. Selector (`UI/Selector`)

A **dropdown-style selector component** for cycling through options with keyboard, gamepad, or mouse input. Perfect for menus, settings panels, and any UI that needs to let users pick from a list.

### Purpose

- Cycle through options (Easy, Medium, Hard; English, Portuguese; Fullscreen, Windowed, etc.)
- Keyboard/gamepad/mouse support (arrow keys, analog stick, buttons)
- Visual feedback (shows selected option, highlights buttons)
- Callback system for responding to selection changes

### Components

#### OptionSelector.cs

The main component that manages option selection logic.

```csharp
public class OptionSelector : Selectable {
    [SerializeField] private TextMeshProUGUI _value;          // Displays current option
    [SerializeField] private Button _leftButton;              // Navigate left
    [SerializeField] private Button _rightButton;             // Navigate right
    [SerializeField] private InputActionReference _nextInput; // Keyboard/gamepad next
    [SerializeField] private InputActionReference _previousInput; // Previous

    public Action<int> OnValueChanged;                        // Callback when selection changes
}
```

### Usage

#### Basic Setup

```csharp
public class SettingsPanel : MonoBehaviour {
    [SerializeField] private OptionSelector _difficultySelector;

    private void Start() {
        // Initialize with options
        _difficultySelector.Initialize(
            new List<string> { "Easy", "Normal", "Hard", "Nightmare" },
            startIndex: 1  // Default to "Normal"
        );

        // Listen for changes
        _difficultySelector.OnValueChanged += OnDifficultySelected;
    }

    private void OnDifficultySelected(int selectedIndex) {
        Debug.Log($"Difficulty changed to: {selectedIndex}");
        ApplyDifficulty(selectedIndex);
    }
}
```

#### Input Methods

Players can interact with OptionSelector in multiple ways:

```
1. Keyboard:
   Left Arrow  → Previous option
   Right Arrow → Next option

2. Gamepad:
   D-Pad Left  → Previous option
   D-Pad Right → Next option
   (or Left/Right analog stick)

3. Mouse:
   Click left button  → Previous
   Click right button → Next
```

#### Advanced: Get Current Selection

```csharp
// After initialization
int selectedIndex = _selector.CurrentIndex;      // e.g., 2
string selectedText = _selector.CurrentValue;    // e.g., "Hard"
```

### Implementation Details

#### Hierarchy

```
Canvas
└── SettingsPanel
    └── DifficultySelector (OptionSelector Component)
        ├── LeftButton (Button)
        ├── DisplayText (TextMeshProUGUI)
        └── RightButton (Button)
```

#### Event Flow

```
Player presses Right Arrow
    ↓
InputAction detected
    ↓
OptionSelector.HandleNextInput()
    ↓
Update internal index
    ↓
Update display text
    ↓
Flash button visual
    ↓
OnValueChanged callback fired
    ↓
SettingsPanel responds (e.g., apply new difficulty)
```

#### Visual Feedback

When the user selects an option:

```csharp
// Button flashes (brief highlight)
StartCoroutine(FlashButton(_rightButton));
// Highlight duration: 0.12 seconds
// Shows user their input was registered
```

### Prefab Setup

The prefab `Selector.prefab` is pre-configured with:
- OptionSelector component
- TextMeshProUGUI for display
- Left/Right buttons for manual selection
- Input action references (if using InputSystem)
- Layout and styling

**To use:**

1. Drag `Selector.prefab` into your Canvas
2. Assign InputActionReferences in Inspector (or leave null for manual buttons only)
3. Call `Initialize()` with your options in Start()
4. Subscribe to `OnValueChanged` event

### Common Patterns

#### Pattern 1: Game Difficulty Selection

```csharp
public class DifficultySelector : MonoBehaviour {
    private OptionSelector _selector;

    private List<string> difficulties = new() { "Easy", "Normal", "Hard" };
    private List<float> healthMultipliers = new() { 0.5f, 1f, 2f };
    private List<float> damageMultipliers = new() { 0.5f, 1f, 1.5f };

    private void Start() {
        _selector = GetComponentInChildren<OptionSelector>();
        _selector.Initialize(difficulties, startIndex: 1);
        _selector.OnValueChanged += SetDifficulty;
    }

    private void SetDifficulty(int index) {
        GameSettings.EnemyHealthMultiplier = healthMultipliers[index];
        GameSettings.EnemyDamageMultiplier = damageMultipliers[index];
        Debug.Log($"Difficulty set to: {difficulties[index]}");
    }
}
```

#### Pattern 2: Language Selection

```csharp
public class LanguageSelector : MonoBehaviour {
    private OptionSelector _selector;

    private void Start() {
        _selector = GetComponentInChildren<OptionSelector>();
        _selector.Initialize(
            new List<string> { "Português", "English" },
            startIndex: 0
        );
        _selector.OnValueChanged += ChangeLanguage;
    }

    private void ChangeLanguage(int index) {
        var language = (Language)index;
        ServiceLocator.Get<ISettingsService>().CurrentLanguage = language;
        RefreshUIText();
    }
}
```

#### Pattern 3: Resolution Selection

```csharp
public class ResolutionSelector : MonoBehaviour {
    private OptionSelector _selector;

    private List<(int width, int height)> resolutions = new() {
        (1280, 720),
        (1920, 1080),
        (2560, 1440)
    };

    private void Start() {
        _selector = GetComponentInChildren<OptionSelector>();
        _selector.Initialize(
            new List<string> { "720p", "1080p", "1440p" },
            startIndex: 1
        );
        _selector.OnValueChanged += ApplyResolution;
    }

    private void ApplyResolution(int index) {
        var (width, height) = resolutions[index];
        Screen.SetResolution(width, height, true);
    }
}
```

### Best Practices

#### 1. Always Call Initialize

❌ **Bad:**
```csharp
// Selector has no options
_selector.OnValueChanged += OnChanged;
// Nothing happens—no options to select
```

✅ **Good:**
```csharp
_selector.Initialize(options, startIndex: 0);
_selector.OnValueChanged += OnChanged;
```

#### 2. Use Meaningful Option Text

❌ **Bad:**
```csharp
_selector.Initialize(new List<string> { "0", "1", "2" });
// What do these numbers mean?
```

✅ **Good:**
```csharp
_selector.Initialize(new List<string> { "Easy", "Normal", "Hard" });
// Clear intent
```

#### 3. Mirror Game State in UI

✅ **Good:**
```csharp
private void Start() {
    var currentDifficulty = GameSettings.CurrentDifficulty;
    _selector.Initialize(difficulties, startIndex: currentDifficulty);
}
// UI reflects current setting
```

#### 4. Provide Visual Feedback

✅ **Good:**
```csharp
// Selector already flashes button on input
// This is automatic—just use the prefab
```

#### 5. Pair Selector with Labels

✅ **Good:**
```csharp
// In Canvas:
// Label: "Difficulty"
// Selector: [Easy] <- [Normal] -> [Hard]
//                     Selected

// Both label and selector visible = clear context
```

### Customization

#### Change Button Colors

In Inspector, modify the Buttons' color states:
- Normal Color: Default appearance
- Highlighted Color: When hovering
- Pressed Color: When clicking (flash briefly)

#### Change Display Font

Select the TextMeshProUGUI component and assign a different font asset.

#### Add More Options

```csharp
// At runtime (if game supports it)
var newOptions = new List<string> { "Option1", "Option2", "Option3" };
_selector.Initialize(newOptions, startIndex: 0);
```

### Input System Integration

OptionSelector uses Unity's InputSystem:

```csharp
[SerializeField] private InputActionReference _nextInput;      // e.g., D-Pad Right
[SerializeField] private InputActionReference _previousInput;  // e.g., D-Pad Left
```

If InputActionReferences are assigned, the component automatically listens to those actions. If not assigned, only manual button clicks work (still functional).

### Accessibility Considerations

- **Keyboard & Gamepad**: Full support for non-mouse input
- **Button Feedback**: Visual flash indicates input was received
- **Clear Display**: Selected option always visible in text
- **No Forced Timing**: Players take as long as needed to select

---

## Creating New UI Components

To add a new reusable UI component:

### Step 1: Create Script Folder

```
UI/NewComponent/
├── Scripts/
│   └── NewComponent.cs
└── Prefabs/
    └── NewComponent.prefab
```

### Step 2: Implement Component

```csharp
// In Scripts/NewComponent.cs
using UnityEngine;
using UnityEngine.UI;

namespace FifthSemester.Framework.UI {
    public class NewComponent : Selectable {
        [SerializeField] private Text _display;
        
        public void Initialize(string value) {
            _display.text = value;
        }
    }
}
```

### Step 3: Create Prefab

1. Create a GameObject with your component
2. Configure visuals (buttons, text, etc.)
3. Save as prefab in Prefabs/ folder

### Step 4: Document Usage

Add README or code comments explaining how to use the component.

---

## Best Practices for All UI Components

### 1. Extend Selectable

```csharp
// Inherit from Selectable for consistency with Unity UI
public class MyUIComponent : Selectable { }
```

### 2. Use Callbacks, Not Direct Coupling

✅ **Good:**
```csharp
public Action<int> OnValueChanged;

private void ValueChanged() {
    OnValueChanged?.Invoke(newValue);
}
```

❌ **Bad:**
```csharp
// Direct reference to handler
private SettingsPanel _settingsPanel;

private void ValueChanged() {
    _settingsPanel.OnSettingChanged(newValue);  // Tight coupling
}
```

### 3. Provide Inspector Setup

```csharp
[Header("UI References")]
[SerializeField] private TextMeshProUGUI _display;
[SerializeField] private Button _confirmButton;

[Header("Input")]
[SerializeField] private InputActionReference _confirmInput;
```

### 4. Test in Isolation

Create test scenes that only use the UI component without full game logic.

### 5. Document with Examples

Include code examples in comments or README:

```csharp
/// <example>
/// <code>
/// var selector = GetComponent&lt;OptionSelector&gt;();
/// selector.Initialize(new List&lt;string&gt; { "A", "B", "C" });
/// selector.OnValueChanged += (index) => Debug.Log(index);
/// </code>
/// </example>
```

---

## Summary

The **UI** subsystem provides:
- **Reusable components**: OptionSelector for common patterns
- **Input agnostic**: Works with keyboard, gamepad, and mouse
- **Callback-based**: No tight coupling to game logic
- **Extensible**: Add new components following the same pattern
- **Accessible**: Keyboard and gamepad support built-in

By using Framework UI components, PHOTOSSYNC achieves:
- **Consistency**: Same interaction patterns across menus
- **Maintainability**: UI code is isolated and testable
- **Reusability**: Components work across different features
- **Portability**: Components can be used in other projects

**See also:**
- [Framework/README.md](../README.md) - Framework overview
- UI implementations in Gameplay/Menu/ and Gameplay/Settings/
