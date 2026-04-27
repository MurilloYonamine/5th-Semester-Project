using FifthSemester.Core.Enums;
using FifthSemester.Core.Services;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;
using UnityEngine.UI;

namespace FifthSemester.Gameplay.Menu {
    public class SettingsMenuView : MonoBehaviour {
        private IMenuService _menuService;

        [SerializeField] private GameObject _focusFirstElement;

        [Header("Buttons")]
        [SerializeField] private Button _audioSettingsButton;
        [SerializeField] private Button _graphicsSettingsButton;
        [SerializeField] private Button _gameplaySettingsButton;
        [SerializeField] private Button _screenSettingsButton;
        [SerializeField] private Button _backButton;

        [Header("Reset")]
        [SerializeField] private Button _resetDefaultsButton;
        [SerializeField] private SettingsDefaults _settingsDefaults;
        private ISettingsService _settingsService;

        [Header("Submenu Views")]
        private AudioSettingsView _audioSettingsView;
        private GraphicsSettingsView _graphicsSettingsView;
        private GameplaySettingsView _gameplaySettingsView;
        private ScreenSettingsView _screenSettingsView;


        private void Start() {
            _menuService = ServiceLocator.Get<IMenuService>();
            _settingsService = ServiceLocator.Get<ISettingsService>();

            _audioSettingsView = _menuService.GetView(MenuScreen.Settings_Audio).GetComponent<AudioSettingsView>();
            _graphicsSettingsView = _menuService.GetView(MenuScreen.Settings_Graphics).GetComponent<GraphicsSettingsView>();
            _gameplaySettingsView = _menuService.GetView(MenuScreen.Settings_Gameplay).GetComponent<GameplaySettingsView>();
            _screenSettingsView = _menuService.GetView(MenuScreen.Settings_Screen).GetComponent<ScreenSettingsView>();

            _menuService.Register(MenuScreen.Settings, gameObject);

            _audioSettingsButton.onClick.AddListener(OpenAudioSettings);
            _graphicsSettingsButton.onClick.AddListener(OpenGraphicsSettings);
            _gameplaySettingsButton.onClick.AddListener(OpenGameplaySettings);
            _screenSettingsButton.onClick.AddListener(OpenScreenSettings);
            _resetDefaultsButton.onClick.AddListener(OnResetDefaults);
            _backButton.onClick.AddListener(OnBack);
        }
        public void OnResetDefaults() {
            if (_settingsDefaults == null || _settingsService == null) return;

            // Reset values
            _settingsService.MasterVolume = _settingsDefaults.MasterVolume;
            _settingsService.MusicVolume = _settingsDefaults.MusicVolume;
            _settingsService.SFXVolume = _settingsDefaults.SFXVolume;
            _settingsService.AmbienceVolume = _settingsDefaults.AmbienceVolume;
            _settingsService.ForceMonoAudio = _settingsDefaults.ForceMonoAudio;

            _settingsService.BarrelDistortion = _settingsDefaults.BarrelDistortion;
            _settingsService.Dithering = _settingsDefaults.Dithering;
            _settingsService.Pixelation = _settingsDefaults.Pixelation;
            _settingsService.RollingBands = _settingsDefaults.RollingBands;
            _settingsService.Scanlines = _settingsDefaults.Scanlines;
            _settingsService.VHSEffect = _settingsDefaults.VHSEffect;

            _settingsService.FrameRate = _settingsDefaults.FrameRate;
            _settingsService.IsFullscreen = _settingsDefaults.IsFullscreen;
            _settingsService.ResolutionIndex = _settingsDefaults.ResolutionIndex;

            _settingsService.Language = _settingsDefaults.Language;
            _settingsService.InvertYAxis = _settingsDefaults.InvertYAxis;
            _settingsService.Sensibility = _settingsDefaults.Sensibility;

            // Aplicar imediatamente nos serviços
            var graphicsService = ServiceLocator.Get<IGraphicsService>();
            if (graphicsService != null) {
                graphicsService.SetBarrelDistortion(_settingsDefaults.BarrelDistortion);
                graphicsService.SetDithering(_settingsDefaults.Dithering);
                graphicsService.SetPixelation(_settingsDefaults.Pixelation);
                graphicsService.SetRollingBands(_settingsDefaults.RollingBands);
                graphicsService.SetScanlines(_settingsDefaults.Scanlines);
                graphicsService.SetVHSEffect(_settingsDefaults.VHSEffect);
            }
            var screenService = ServiceLocator.Get<IScreenService>();
            if (screenService != null) {
                var resArray = _settingsService.AvailableResolutions;
                int idx = _settingsDefaults.ResolutionIndex;
                if (resArray != null && idx >= 0 && idx < resArray.Length) {
                    var res = resArray[idx];
                    screenService.SetResolution(res.x, res.y, _settingsDefaults.IsFullscreen);
                }
                screenService.SetFullscreen(_settingsDefaults.IsFullscreen);
                screenService.SetFrameRate(_settingsDefaults.FrameRate);
            }

            _audioSettingsView?.RefreshUI();
            _graphicsSettingsView?.RefreshUI();
            _gameplaySettingsView?.RefreshUI();
            _screenSettingsView?.RefreshUI();
        }

        private void OnDestroy() {
            _menuService?.Unregister(MenuScreen.Settings);
        }

        private void OnEnable() {
            EventSystem.current.SetSelectedGameObject(null);

            if (_focusFirstElement != null) {
                EventSystem.current.SetSelectedGameObject(_focusFirstElement);
            }

            InputSystem.onAnyButtonPress.Call(OnAnyInput);
        }

        public void OpenAudioSettings() => _menuService.Show(MenuScreen.Settings_Audio);
        public void OpenGraphicsSettings() => _menuService.Show(MenuScreen.Settings_Graphics);
        public void OpenGameplaySettings() => _menuService.Show(MenuScreen.Settings_Gameplay);
        public void OpenScreenSettings() => _menuService.Show(MenuScreen.Settings_Screen);

        public void OnBack() {
            _menuService.Show(MenuScreen.MainMenu);
        }

        private void OnAnyInput(InputControl control) {
            if (control.device is Gamepad && EventSystem.current.currentSelectedGameObject == null) {
                EventSystem.current.SetSelectedGameObject(_focusFirstElement);
            }
        }
    }
}