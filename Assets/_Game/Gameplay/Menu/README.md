# The `Menu` Directory: UI Navigation & Settings

The **Menu** directory manages **menu screens, navigation, and settings**—main menu, pause menu, settings panel, and user preferences persistence.

---

## Purpose

Menu System provides:
- **Screen navigation**: Switch between menu screens without scene loading
- **Settings management**: Audio, graphics, gameplayvolume, resolution
- **Prefab-based UI**: Reusable menu screen components
- **Settings persistence**: Save/load user preferences
- **State integration**: Pause/resume through menu

---

## Architecture

```
MenuService.cs (orchestrator)
├── View/ (UI screens)
│   ├── MainMenuView
│   ├── PauseMenuView
│   ├── SettingsMenuView
│   ├── GraphicsView
│   ├── AudioView
│   └── CreditsView
├── Services/
│   ├── GameplayService (game-specific settings)
│   ├── GraphicsService (visual settings)
│   ├── AudioService (sound settings)
│   └── ScreenService (resolution/fullscreen)
└── Default/
    ├── GameplayDefault.json
    ├── GraphicsDefault.json
    ├── AudioDefault.json
    └── ScreenDefault.json
```

---

## Key Files

### `MenuService.cs`

Central menu manager handling screen transitions.

```csharp
public class MenuService : MonoBehaviour, IMenuService {
    [SerializeField] private Canvas _menuCanvas;
    [SerializeField] private MainMenuView _mainMenuPrefab;
    [SerializeField] private PauseMenuView _pauseMenuPrefab;
    [SerializeField] private SettingsMenuView _settingsMenuPrefab;

    private Dictionary<MenuScreen, BaseMenuView> _screens = new();
    private BaseMenuView _currentScreen;
    private MenuScreen _previousScreen;
    private IEventBus _eventBus;

    private void Start() {
        ServiceLocator.Register<IMenuService>(this);
        _eventBus = ServiceLocator.Get<IEventBus>();
        
        // Load all menu views
        InstantiateMenuScreens();
        
        // Default to main menu
        ShowScreen(MenuScreen.Main);
    }

    private void InstantiateMenuScreens() {
        // Instantiate and cache all menu screens
        var mainMenu = Instantiate(_mainMenuPrefab, _menuCanvas.transform);
        _screens[MenuScreen.Main] = mainMenu;
        mainMenu.gameObject.SetActive(false);

        var pauseMenu = Instantiate(_pauseMenuPrefab, _menuCanvas.transform);
        _screens[MenuScreen.Pause] = pauseMenu;
        pauseMenu.gameObject.SetActive(false);

        // ... more screens
    }

    public void ShowScreen(MenuScreen screen) {
        // Hide previous screen
        if (_currentScreen != null) {
            _currentScreen.gameObject.SetActive(false);
        }

        // Show new screen
        if (_screens.TryGetValue(screen, out var newScreen)) {
            _previousScreen = CurrentScreen;
            _currentScreen = newScreen;
            _currentScreen.gameObject.SetActive(true);

            Debug.Log($"Showing menu screen: {screen}");
        }
    }

    public void GoBack() {
        ShowScreen(_previousScreen);
    }

    public MenuScreen CurrentScreen { get; private set; }
}
```

---

### `View/MainMenuView.cs`

Main menu screen with play/settings/quit buttons.

```csharp
public class MainMenuView : BaseMenuView {
    [SerializeField] private Button _playButton;
    [SerializeField] private Button _settingsButton;
    [SerializeField] private Button _creditsButton;
    [SerializeField] private Button _quitButton;

    private IMenuService _menuService;

    private void Start() {
        _menuService = ServiceLocator.Get<IMenuService>();

        _playButton.onClick.AddListener(OnPlayClicked);
        _settingsButton.onClick.AddListener(OnSettingsClicked);
        _creditsButton.onClick.AddListener(OnCreditsClicked);
        _quitButton.onClick.AddListener(OnQuitClicked);
    }

    private void OnPlayClicked() {
        // Start game
        SceneManager.LoadScene("GameplayScene");
    }

    private void OnSettingsClicked() {
        _menuService.ShowScreen(MenuScreen.Settings);
    }

    private void OnCreditsClicked() {
        _menuService.ShowScreen(MenuScreen.Credits);
    }

    private void OnQuitClicked() {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
}
```

---

### `View/PauseMenuView.cs`

Pause menu shown during gameplay.

```csharp
public class PauseMenuView : BaseMenuView {
    [SerializeField] private Button _resumeButton;
    [SerializeField] private Button _settingsButton;
    [SerializeField] private Button _mainMenuButton;

    private IMenuService _menuService;
    private IEventBus _eventBus;

    private void Start() {
        _menuService = ServiceLocator.Get<IMenuService>();
        _eventBus = ServiceLocator.Get<IEventBus>();

        _resumeButton.onClick.AddListener(OnResumeClicked);
        _settingsButton.onClick.AddListener(OnSettingsClicked);
        _mainMenuButton.onClick.AddListener(OnMainMenuClicked);
    }

    private void OnResumeClicked() {
        // Exit pause menu
        _eventBus.Publish<PauseToggleRequestedEvent>(new());
        gameObject.SetActive(false);
    }

    private void OnSettingsClicked() {
        _menuService.ShowScreen(MenuScreen.PauseSettings);
    }

    private void OnMainMenuClicked() {
        // Unpause first
        var gameState = ServiceLocator.Get<IGameStateService>();
        if (gameState.CurrentState == GameState.Paused) {
            gameState.ChangeState(GameState.Gameplay);
        }

        // Load main menu
        SceneManager.LoadScene("MainMenuScene");
    }
}
```

---

### `View/SettingsMenuView.cs`

Settings menu with tabs for graphics, audio, gameplay.

```csharp
public class SettingsMenuView : BaseMenuView {
    [SerializeField] private Button _audioTabButton;
    [SerializeField] private Button _graphicsTabButton;
    [SerializeField] private Button _gameplayTabButton;
    [SerializeField] private Button _backButton;

    [SerializeField] private AudioSettingsPanel _audioPanel;
    [SerializeField] private GraphicsSettingsPanel _graphicsPanel;
    [SerializeField] private GameplaySettingsPanel _gameplayPanel;

    private IMenuService _menuService;

    private void Start() {
        _menuService = ServiceLocator.Get<IMenuService>();

        _audioTabButton.onClick.AddListener(() => ShowTab(_audioPanel));
        _graphicsTabButton.onClick.AddListener(() => ShowTab(_graphicsPanel));
        _gameplayTabButton.onClick.AddListener(() => ShowTab(_gameplayPanel));
        _backButton.onClick.AddListener(() => _menuService.GoBack());

        // Default tab
        ShowTab(_audioPanel);
    }

    private void ShowTab(SettingsPanel panel) {
        _audioPanel.gameObject.SetActive(false);
        _graphicsPanel.gameObject.SetActive(false);
        _gameplayPanel.gameObject.SetActive(false);

        panel.gameObject.SetActive(true);
    }
}
```

---

### `Services/SettingsService.cs`

Centralized settings storage and persistence.

```csharp
[System.Serializable]
public class Settings {
    public AudioSettings audioSettings = new();
    public GraphicsSettings graphicsSettings = new();
    public GameplaySettings gameplaySettings = new();
}

public class SettingsService : MonoBehaviour {
    private Settings _settings;
    private const string SETTINGS_PATH = "settings.json";

    private void Start() {
        ServiceLocator.Register<ISettingsService>(this);
        LoadSettings();
    }

    public void LoadSettings() {
        if (File.Exists(SETTINGS_PATH)) {
            string json = File.ReadAllText(SETTINGS_PATH);
            _settings = JsonConvert.DeserializeObject<Settings>(json);
        } else {
            _settings = new Settings();
            ApplyDefaultSettings();
        }

        ApplySettings();
    }

    public void SaveSettings() {
        string json = JsonConvert.SerializeObject(_settings, Formatting.Indented);
        File.WriteAllText(SETTINGS_PATH, json);
    }

    private void ApplySettings() {
        // Apply audio settings
        AudioListener.volume = _settings.audioSettings.masterVolume;

        // Apply graphics settings
        QualitySettings.SetQualityLevel(_settings.graphicsSettings.qualityLevel);
        Screen.SetResolution(
            _settings.graphicsSettings.resolution.x,
            _settings.graphicsSettings.resolution.y,
            _settings.graphicsSettings.isFullscreen
        );
    }

    public T GetSetting<T>(string key) where T : class {
        // Get specific setting
        return null;
    }

    public void SetSetting<T>(string key, T value) where T : class {
        // Set specific setting
        SaveSettings();
    }
}
```

---

## Menu Flow

```
Game Starts
    ↓
MainMenuView
├─ Play → Load gameplay scene
├─ Settings → SettingsMenuView
│   ├─ Audio tab
│   ├─ Graphics tab
│   └─ Gameplay tab
├─ Credits → CreditsView
└─ Quit → Exit game

In Gameplay
├─ Press Esc → GameStateService.ChangeState(Paused)
├─ PauseMenuView shows
│   ├─ Resume → ChangeState(Gameplay)
│   ├─ Settings → SettingsMenuView
│   └─ Main Menu → Load menu scene
└─ Continue game
```

---

## Settings Structure

### AudioSettings
```json
{
  "masterVolume": 0.8,
  "musicVolume": 0.7,
  "sfxVolume": 0.9,
  "ambienceVolume": 0.5
}
```

### GraphicsSettings
```json
{
  "qualityLevel": 2,
  "resolution": [1920, 1080],
  "isFullscreen": true,
  "targetFramerate": 60
}
```

### GameplaySettings
```json
{
  "difficulty": "Normal",
  "language": "English",
  "subtitles": true
}
```

---

## Best Practices

### 1. Cache Menu Screens

✅ **Good:**
```csharp
// Instantiate once, reuse
var screen = Instantiate(_prefab);
_screens[MenuScreen.Main] = screen;
screen.SetActive(false);  // Just enable/disable
```

❌ **Bad:**
```csharp
// Recreate every time
void ShowSettings() {
    Instantiate(_settingsPrefab);  // Memory leak
}
```

### 2. Use JSON for Settings

✅ **Good:**
```csharp
string json = JsonConvert.SerializeObject(_settings);
File.WriteAllText("settings.json", json);
```

### 3. Provide Default Settings

✅ **Good:**
```csharp
// Load defaults if no save exists
if (!File.Exists(SETTINGS_PATH)) {
    _settings = LoadDefaults();
}
```

### 4. Tab-Based Settings UI

✅ **Good:**
```
Settings Menu
├─ Audio Tab ← selected
│   ├─ Master Volume
│   ├─ Music Volume
│   └─ SFX Volume
├─ Graphics Tab
├─ Gameplay Tab
```

### 5. Save Immediately

✅ **Good:**
```csharp
// When user changes volume
audioSlider.onValueChanged.AddListener((value) => {
    _settings.masterVolume = value;
    SaveSettings();
});
```

---

## Common Patterns

### Main Menu Flow

```csharp
public void ShowMainMenu() {
    _menuService.ShowScreen(MenuScreen.Main);
    Time.timeScale = 1f;  // Ensure time running
}
```

### Pause Menu Flow

```csharp
public void OnPauseToggle() {
    if (CurrentState == GameState.Paused) {
        ChangeState(GameState.Gameplay);
        _menuService.ShowScreen(MenuScreen.Hidden);
    } else {
        ChangeState(GameState.Paused);
        _menuService.ShowScreen(MenuScreen.Pause);
    }
}
```

### Settings Apply Immediately

```csharp
public class VolumeSlider : MonoBehaviour {
    private void Start() {
        slider.onValueChanged.AddListener(OnVolumeChanged);
    }

    private void OnVolumeChanged(float value) {
        AudioListener.volume = value;  // Immediate
        _settingsService.SetSetting("masterVolume", value);
    }
}
```

---

## Extending Menu

### Add New Menu Screen

```csharp
public class CreditsView : BaseMenuView {
    [SerializeField] private Button _backButton;

    private void Start() {
        _backButton.onClick.AddListener(GoBack);
    }

    private void GoBack() {
        ServiceLocator.Get<IMenuService>().GoBack();
    }
}
```

### Add Resolution Options

```csharp
public class GraphicsSettingsPanel : SettingsPanel {
    [SerializeField] private Dropdown _resolutionDropdown;

    private void Start() {
        var resolutions = Screen.resolutions;
        
        foreach (var res in resolutions) {
            _resolutionDropdown.options.Add(
                new Dropdown.OptionData($"{res.width}x{res.height}")
            );
        }

        _resolutionDropdown.onValueChanged.AddListener(OnResolutionChanged);
    }

    private void OnResolutionChanged(int index) {
        var res = Screen.resolutions[index];
        Screen.SetResolution(res.width, res.height, Screen.fullScreen);
    }
}
```

---

## Summary

The **Menu** system provides:
- **Screen navigation**: Easy menu screen switching
- **Settings management**: Centralized preference storage
- **Persistence**: Save/load user settings
- **Tab-based UI**: Organized settings panels
- **State integration**: Pause/resume gameplay

By using MenuService:
- **Designers** create menu screens easily
- **Players** configure preferences
- **Settings persist** across sessions
- **Menu flows** are consistent and predictable

**See also:**
- [Gameplay/README.md](../README.md) - Gameplay features
- [Core/Audio/README.md](../../Core/Audio/README.md) - Audio settings
