# The `Enums` Directory: Shared Type Definitions

The **Enums** directory contains global enumeration types used across multiple systems and features. These are the **shared vocabulary** of the game—common types that everyone agrees on.

---

## Key Files

### `GameState.cs`

Defines all possible states the game can be in. Used by `GameStateService` to track high-level game flow.

```csharp
namespace FifthSemester.Core.States
{
    public enum GameState
    {
        Gameplay,     // Active player control in the game world
        MainMenu,     // Main menu screen
        Dialogue,     // Active dialogue/cinematic playing
        Cutscene,     // Non-interactive story sequence
        Paused        // Game paused (UI overlays, input disabled)
    }
}
```

#### Usage

```csharp
// Get current state
GameState currentState = ServiceLocator.Get<IGameStateService>().CurrentState;

// Transition to a new state
if (currentState == GameState.Gameplay)
{
    gameStateService.ChangeState(GameState.Paused);
}

// Subscribe to state changes
eventBus.Subscribe<GameStateChangedEvent>(OnGameStateChanged);

private void OnGameStateChanged(GameStateChangedEvent evt)
{
    if (evt.CurrentState == GameState.MainMenu)
    {
        // UI responds
    }
}
```

#### State Transitions

```
MainMenu → Gameplay
            ↓
         Dialogue → Gameplay → Paused → Gameplay
            ↓
         Cutscene → Gameplay
```

---

### `Language.cs`

Defines supported languages for dialogue, UI text, and localization.

```csharp
namespace FifthSemester.Core.Enums
{
    public enum Language
    {
        Portugues = 0,  // Portuguese (Brazilian)
        English         // English
    }
}
```

#### Usage

```csharp
// Get user's language preference
Language userLanguage = ServiceLocator.Get<ISettingsService>().CurrentLanguage;

// Load dialogue in the correct language
string dialogueLine = GetDialogueText(dialogueId, userLanguage);

// Localize UI elements
switch (currentLanguage)
{
    case Language.Portugues:
        uiLabel.text = "Iniciar Jogo";
        break;
    case Language.English:
        uiLabel.text = "Start Game";
        break;
}
```

#### Extending Languages

To add a new language:

```csharp
public enum Language
{
    Portugues = 0,
    English,
    Español      // New language
}
```

Then update all localization systems to handle the new language.

---

### `MenuScreen.cs`

Defines all possible menu screens in the game. Used by `MenuService` to manage navigation and transitions.

```csharp
namespace FifthSemester.Core.Enums
{
    public enum MenuScreen
    {
        None = 0,                  // No menu active
        MainMenu,                  // Main menu
        PauseMenu,                 // Pause menu
        Settings,                  // Settings parent screen
        Credits,                   // Credits screen
        Settings_Audio,            // Audio settings submenu
        Settings_Graphics,         // Graphics settings submenu
        Settings_Screen,           // Screen/resolution submenu
        Settings_Gameplay          // Gameplay settings submenu
    }
}
```

#### Hierarchy

```
None
├── MainMenu
│   ├── Settings
│   │   ├── Settings_Audio
│   │   ├── Settings_Graphics
│   │   ├── Settings_Screen
│   │   └── Settings_Gameplay
│   └── Credits
└── PauseMenu
    └── Settings
        ├── Settings_Audio
        ├── Settings_Graphics
        ├── Settings_Screen
        └── Settings_Gameplay
```

#### Usage

```csharp
// Navigate to settings
IMenuService menu = ServiceLocator.Get<IMenuService>();
menu.NavigateTo(MenuScreen.Settings_Audio);

// Check current screen
MenuScreen currentScreen = menu.CurrentScreen;

if (currentScreen == MenuScreen.Settings_Audio)
{
    audioSlider.gameObject.SetActive(true);
}

// Go back
menu.GoBack();  // Returns to previous screen
```

#### Navigation Pattern

```csharp
public class MenuController : MonoBehaviour
{
    private IMenuService _menu;

    private void Start()
    {
        _menu = ServiceLocator.Get<IMenuService>();
    }

    public void OnSettingsClicked()
    {
        _menu.NavigateTo(MenuScreen.Settings);
    }

    public void OnAudioClicked()
    {
        _menu.NavigateTo(MenuScreen.Settings_Audio);
    }

    public void OnBackClicked()
    {
        _menu.GoBack();
    }
}
```

---

## Enum Design Principles

### 1. Use Meaningful Names

❌ **Bad:**
```csharp
public enum State
{
    A, B, C  // What do these mean?
}
```

✅ **Good:**
```csharp
public enum GameState
{
    Gameplay,
    MainMenu,
    Paused
}
```

### 2. Provide Explicit Zero Values

❌ **Bad:**
```csharp
public enum Language
{
    English,    // 0
    French      // 1
}
```

✅ **Good:**
```csharp
public enum Language
{
    None = 0,      // Explicit default
    English = 1,
    French = 2
}
```

### 3. Group Related Enums

❌ **Bad:**
```csharp
// Scattered all over the place
public enum State1 { ... }
public enum State2 { ... }
```

✅ **Good:**
```csharp
// All in one directory, organized by domain
// Enums/GameState.cs
// Enums/Language.cs
// Enums/MenuScreen.cs
```

### 4. Use Descriptive Suffixes

✅ **Good:**
```csharp
public enum GameState { ... }      // Suffix: State
public enum MenuScreen { ... }     // Suffix: Screen
public enum Language { ... }       // Suffix: Language
```

---

## Best Practices

### 1. Don't Use Enums for String Comparisons

❌ **Bad:**
```csharp
if (screenName == "SettingsAudio")
{
    // Fragile—typos won't be caught at compile-time
}
```

✅ **Good:**
```csharp
if (currentScreen == MenuScreen.Settings_Audio)
{
    // Type-safe, no typos possible
}
```

### 2. Use Enums in Dictionaries for Type-Safe Lookups

❌ **Bad:**
```csharp
private Dictionary<string, Action> screenHandlers = new()
{
    { "SettingsAudio", OnSettingsAudio },  // Typo risk
};
```

✅ **Good:**
```csharp
private Dictionary<MenuScreen, Action> screenHandlers = new()
{
    { MenuScreen.Settings_Audio, OnSettingsAudio },  // Type-safe
};
```

### 3. Use Switch Statements for Enum Logic

❌ **Bad:**
```csharp
if (state == GameState.Gameplay) { ... }
else if (state == GameState.MainMenu) { ... }
else if (state == GameState.Paused) { ... }
```

✅ **Good:**
```csharp
switch (state)
{
    case GameState.Gameplay:
        // Handle gameplay
        break;
    case GameState.MainMenu:
        // Handle menu
        break;
    case GameState.Paused:
        // Handle pause
        break;
}
```

### 4. Document State Transitions

✅ **Good:**
```csharp
/// <summary>
/// Transitions from current game state to a new state.
/// Valid transitions:
/// - MainMenu → Gameplay
/// - Gameplay → Paused or Dialogue or GameOver
/// - Paused → Gameplay or MainMenu
/// </summary>
public void ChangeState(GameState newState)
{
    // Validation logic
}
```

---

## Adding New Enum Values

### To Add a New GameState:

1. **Update `GameState.cs`**:
```csharp
public enum GameState
{
    Gameplay,
    MainMenu,
    Dialogue,
    Cutscene,
    Paused,
    GameOver        // New state
}
```

2. **Update all handlers**:
   - `GameStateService` - handle transition
   - `InputService` - handle input enable/disable
   - `UIManager` - handle UI display
   - Any feature that cares about state changes

3. **Document transition rules**:
```csharp
// Example: Transition from Paused or Gameplay to GameOver
public void TransitionToGameOver()
{
    if (_currentState == GameState.Paused || _currentState == GameState.Gameplay)
    {
        ChangeState(GameState.GameOver);
    }
}
```

### To Add a New Language:

1. **Update `Language.cs`**:
```csharp
public enum Language
{
    Portugues = 0,
    English,
    Español        // New language
}
```

2. **Update localization systems**:
   - Dialogue service
   - UI text localization
   - Settings menus

### To Add a New MenuScreen:

1. **Update `MenuScreen.cs`**:
```csharp
public enum MenuScreen
{
    // ... existing screens ...
    Settings_Keybinds       // New submenu
}
```

2. **Create corresponding UI panel** in the scene/prefab
3. **Update `MenuService`** to handle navigation

---

## Common Enum Conversion Patterns

### Enum to String

```csharp
string screenName = MenuScreen.Settings_Audio.ToString();
// Result: "Settings_Audio"

// Or with custom formatting
string friendlyName = MenuScreen.Settings_Audio.ToString().Replace("_", " ");
// Result: "Settings Audio"
```

### String to Enum

```csharp
if (System.Enum.TryParse<GameState>("Gameplay", out var state))
{
    currentState = state;
}
```

### Enum to Index

```csharp
int index = (int)Language.English;  // 1
```

### Index to Enum

```csharp
Language lang = (Language)0;  // Portugues
```

---

## Summary

The **Enums** directory provides:
- **Type-safe constants** for game states, languages, and screens
- **Compile-time validation** (no typos, invalid states caught at build-time)
- **Self-documenting code** (clear intent from enum values)
- **Centralized vocabulary** that all systems agree on
- **Easy extensibility** for new game states or screens

By using enums instead of strings or magic numbers, PHOTOSSYNC is **safer, more maintainable, and easier to extend**.
