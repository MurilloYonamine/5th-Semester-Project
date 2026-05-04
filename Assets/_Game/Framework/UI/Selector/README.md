# The `Selector` Directory: UI Component (OptionSelector)

The **Selector** directory contains the **OptionSelector component**—a reusable UI widget for selecting from a list of options. It's organized into Scripts and Prefabs for easy reuse across the project.

---

## Directory Structure

```
Selector/
├── Scripts/
│   └── OptionSelector.cs      (Component implementation)
├── Prefabs/
│   └── Selector.prefab        (Pre-configured prefab)
```

---

## Quick Start

### Using the Prefab

1. Drag `Selector.prefab` into your Canvas
2. Configure in Inspector:
   - Assign `_nextInput` and `_previousInput` to InputActions (or leave null)
   - Assign buttons and text display if not already configured
3. In code, call `Initialize()`:

```csharp
var selector = GetComponentInChildren<OptionSelector>();
selector.Initialize(new List<string> { "Option1", "Option2", "Option3" });
selector.OnValueChanged += (index) => Debug.Log(index);
```

---

## OptionSelector.cs

The main component that handles all selection logic.

### Inspector Fields

```csharp
[Header("UI")]
[SerializeField] private TextMeshProUGUI _value;        // Displays current option

[Header("Optional Buttons")]
[SerializeField] private Button _leftButton;            // Prev button
[SerializeField] private Button _rightButton;           // Next button

[Header("Input References")]
[SerializeField] private InputActionReference _nextInput;       // Next action
[SerializeField] private InputActionReference _previousInput;   // Prev action
```

### Public API

```csharp
// Initialize with options
void Initialize(List<string> options, int startIndex = 0);

// Get current selection
int CurrentIndex { get; }
string CurrentValue { get; }

// Move selection
void Next();
void Previous();

// Event callback
public Action<int> OnValueChanged;
```

### Usage Examples

#### Example 1: Difficulty Selector

```csharp
public class DifficultyMenu : MonoBehaviour {
    [SerializeField] private OptionSelector _difficultySelector;

    private void Start() {
        _difficultySelector.Initialize(
            new List<string> { "Easy", "Normal", "Hard", "Nightmare" },
            startIndex: 1  // Default: Normal
        );

        _difficultySelector.OnValueChanged += SetDifficulty;
    }

    private void SetDifficulty(int index) {
        switch (index) {
            case 0: GameSettings.Difficulty = Difficulty.Easy; break;
            case 1: GameSettings.Difficulty = Difficulty.Normal; break;
            case 2: GameSettings.Difficulty = Difficulty.Hard; break;
            case 3: GameSettings.Difficulty = Difficulty.Nightmare; break;
        }
    }
}
```

#### Example 2: Language Selector

```csharp
public class LanguageMenu : MonoBehaviour {
    private OptionSelector _languageSelector;

    private void Start() {
        _languageSelector = GetComponentInChildren<OptionSelector>();
        _languageSelector.Initialize(
            new List<string> { "Português", "English", "Español" },
            startIndex: 0
        );

        _languageSelector.OnValueChanged += ChangeLanguage;
    }

    private void ChangeLanguage(int index) {
        var language = (Language)index;
        ServiceLocator.Get<ISettingsService>().CurrentLanguage = language;
        RefreshAllText();  // Update UI text
    }
}
```

#### Example 3: Resolution Selector with Dynamic Options

```csharp
public class GraphicsMenu : MonoBehaviour {
    private OptionSelector _resolutionSelector;
    
    private List<(int, int)> _resolutions = new() {
        (1280, 720),
        (1920, 1080),
        (2560, 1440),
        (3840, 2160)  // 4K
    };

    private void Start() {
        _resolutionSelector = GetComponentInChildren<OptionSelector>();

        // Create display strings
        var options = new List<string>();
        foreach (var (w, h) in _resolutions) {
            options.Add($"{w}x{h}");
        }

        _resolutionSelector.Initialize(options, startIndex: 1);
        _resolutionSelector.OnValueChanged += ApplyResolution;
    }

    private void ApplyResolution(int index) {
        var (width, height) = _resolutions[index];
        Screen.SetResolution(width, height, Screen.fullScreen);
    }
}
```

---

## Prefab Hierarchy

```
Selector (Prefab Root)
├── LeftButton (Button)
│   └── Text (TextMeshProUGUI)
├── DisplayArea (Image / Panel)
│   └── OptionText (TextMeshProUGUI)
└── RightButton (Button)
    └── Text (TextMeshProUGUI)
```

### Component Setup

| Component | On | Purpose |
|-----------|----|----|
| OptionSelector | Root | Main logic |
| Button (Left) | LeftButton | Navigate to previous |
| Button (Right) | RightButton | Navigate to next |
| TextMeshProUGUI | OptionText | Display current value |
| InputActionReference | Prev/Next | Keyboard/gamepad input |

---

## Input System Integration

### Using InputActionReferences

```csharp
// In Inspector, assign:
_nextInput = <your InputActionAsset>/Player/Right
_previousInput = <your InputActionAsset>/Player/Left
```

The selector automatically listens for these actions when enabled.

### Manual Button Input

If InputActionReferences are not assigned, only button clicks work (still fully functional).

### Input Flow

```
Player Input (Keyboard / Gamepad / Mouse)
    ↓
InputAction triggered
    ↓
HandleNextInput() / HandlePreviousInput()
    ↓
Next() / Previous() called
    ↓
Index updated
    ↓
Display refreshed
    ↓
OnValueChanged callback fired
    ↓
Consuming code responds
```

---

## Customization

### Change Display Format

Override the display update:

```csharp
public class CustomSelector : OptionSelector {
    protected override void UpdateDisplay() {
        // Custom formatting
        _value.text = $"[{_currentIndex + 1}/{_options.Count}] {_options[_currentIndex]}";
    }
}
```

### Change Flash Duration

Modify the button flash coroutine:

```csharp
private IEnumerator FlashButton(Button button) {
    var colors = button.colors;
    var originalColor = button.targetGraphic.color;
    button.targetGraphic.color = colors.pressedColor;
    yield return new WaitForSeconds(0.2f);  // Increase to 0.2 seconds
    button.targetGraphic.color = originalColor;
}
```

### Add Circular Navigation

Make selection wrap around:

```csharp
public void Next() {
    _currentIndex = (_currentIndex + 1) % _options.Count;  // Wrap
    OnValueChanged?.Invoke(_currentIndex);
}

public void Previous() {
    _currentIndex = (_currentIndex - 1 + _options.Count) % _options.Count;  // Wrap
    OnValueChanged?.Invoke(_currentIndex);
}
```

### Add Sound Effects

```csharp
private void Next() {
    _currentIndex = Mathf.Min(_currentIndex + 1, _options.Count - 1);
    ServiceLocator.Get<IAudioService>().PlaySFX("UI/Select");
    OnValueChanged?.Invoke(_currentIndex);
}
```

---

## Best Practices

### 1. Always Initialize Before Use

❌ **Bad:**
```csharp
// Using selector without Initialize
_selector.OnValueChanged += Handler;
// Selector has no options!
```

✅ **Good:**
```csharp
_selector.Initialize(options, startIndex: 0);
_selector.OnValueChanged += Handler;
```

### 2. Reflect Current Game State

✅ **Good:**
```csharp
private void Start() {
    int currentDifficulty = GameSettings.CurrentDifficulty;
    _selector.Initialize(difficulties, startIndex: currentDifficulty);
    // UI matches game state
}
```

### 3. Use Meaningful Strings

❌ **Bad:**
```csharp
_selector.Initialize(new List<string> { "0", "1", "2", "3" });
```

✅ **Good:**
```csharp
_selector.Initialize(new List<string> { "Low", "Medium", "High", "Ultra" });
```

### 4. Pair with Labels

```
[Label: "Graphics Quality"]
[Low] ← [Medium] → [High]

Both visible = clear context
```

### 5. Validate Selection Changes

```csharp
private void OnSelectionChanged(int index) {
    if (!CanSelectOption(index)) {
        Debug.LogWarning($"Cannot select option {index}");
        return;
    }
    
    ApplySelection(index);
}
```

---

## Common Issues

### Issue: Selector shows but input doesn't work

**Solution**: Ensure InputActionReferences are assigned in Inspector (or use buttons only).

### Issue: Index out of bounds

**Solution**: Call `Initialize()` with at least one option before using.

### Issue: Value doesn't update on screen

**Solution**: Check that `_value` (TextMeshProUGUI) is properly assigned in Inspector.

### Issue: Buttons don't flash

**Solution**: Ensure buttons have `interactable = true` in Inspector.

---

## Advanced: Multi-Level Selector

Create a selector that opens sub-selectors:

```csharp
public class MainSettingsMenu : MonoBehaviour {
    private OptionSelector _mainSelector;
    private OptionSelector _audioSubSelector;
    private OptionSelector _graphicsSubSelector;

    private void Start() {
        _mainSelector = GetComponentInChildren<OptionSelector>();
        _mainSelector.Initialize(
            new List<string> { "Audio", "Graphics", "Gameplay" }
        );
        _mainSelector.OnValueChanged += OnMainMenuChanged;
    }

    private void OnMainMenuChanged(int index) {
        switch (index) {
            case 0:
                ShowAudioSettings();
                break;
            case 1:
                ShowGraphicsSettings();
                break;
            case 2:
                ShowGameplaySettings();
                break;
        }
    }

    private void ShowAudioSettings() {
        _audioSubSelector.gameObject.SetActive(true);
        _graphicsSubSelector.gameObject.SetActive(false);
        // ... etc
    }
}
```

---

## Summary

The **Selector** component provides:
- **Option selection widget**: Cycle through options
- **Input agnostic**: Keyboard, gamepad, and mouse
- **Callback system**: Respond to selection changes
- **Customizable display**: Adapt to any UI style
- **Reusable prefab**: Drag-and-drop setup

By using OptionSelector, PHOTOSSYNC achieves:
- **Consistent menus**: Same interaction pattern everywhere
- **Easy implementation**: Minimal code for common UI patterns
- **Accessibility**: Full keyboard and gamepad support
- **Extensibility**: Easy to customize for specific needs

**See also:**
- [UI/README.md](../README.md) - Framework UI overview
- OptionSelector.cs for implementation details
