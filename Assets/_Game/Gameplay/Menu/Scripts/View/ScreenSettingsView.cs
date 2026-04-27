using System.Collections.Generic;
using FifthSemester.Core.Enums;
using FifthSemester.Core.Services;
using UnityEngine;
using UnityEngine.UI;
using FifthSemester.Framework.UI;
using TMPro;
using UnityEngine.EventSystems;

namespace FifthSemester.Gameplay.Menu {
    public class ScreenSettingsView : MonoBehaviour {
        private IMenuService _menuService;
        private ISettingsService _settingsService;
        private IScreenService _screenService;

        [Header("Focus")]
        [SerializeField] private GameObject _focusFirstElement;

        [Header("Selectors")]
        [SerializeField] private OptionSelector _resolutionSelector;
        [SerializeField] private OptionSelector _fpsSelector;
        [SerializeField] private Toggle _fullscreenToggle;

        [Header("Buttons")]
        [SerializeField] private Button _backButton;

        [Header("TMP Values")]
        [SerializeField] private TextMeshProUGUI _fullscreenValue;
        private readonly List<int> _fpsValues = new() { 24, 30, 60, -1 };

        private void Start() {
            _menuService = ServiceLocator.Get<IMenuService>();
            _settingsService = ServiceLocator.Get<ISettingsService>();
            _screenService = new ScreenService();

            _menuService.Register(MenuScreen.Settings_Screen, gameObject);

            _fullscreenToggle.isOn = _settingsService.IsFullscreen;
            _fullscreenToggle.onValueChanged.AddListener(OnFullscreenToggled);

            _resolutionSelector.Initialize(GetResolutionOptions(), _settingsService.ResolutionIndex);
            _resolutionSelector.OnValueChanged += OnResolutionChanged;

            _fpsSelector.Initialize(GetFPSOptions(), _fpsValues.IndexOf(_settingsService.FrameRate));
            _fpsSelector.OnValueChanged += OnFPSChanged;

            _backButton.onClick.AddListener(OnBack);
        }

        private void OnEnable() {
            if (_focusFirstElement != null) {
                EventSystem.current.SetSelectedGameObject(_focusFirstElement);
            }
        }

        private void OnDestroy() {
            _menuService?.Unregister(MenuScreen.Settings_Screen);
        }
        public void OnResolutionChanged(int index) {
            var resolution = Screen.resolutions[index];
            _screenService.SetResolution(resolution.width, resolution.height, _fullscreenToggle.isOn);
        }
        public void OnFPSChanged(int index) {
            int fps = _fpsValues[index];
            _screenService.SetFrameRate(fps);
        }
        private List<string> GetFPSOptions() {
            List<string> options = new();
            foreach (var fps in _fpsValues) {
                options.Add(fps > 0 ? fps.ToString() : "Unlimited");
            }
            return options;
        }
        private List<string> GetResolutionOptions() {
            List<string> options = new();
            foreach (var res in _settingsService.AvailableResolutions) {
                options.Add($"{res.x}x{res.y}");
            }
            return options;
        }
        public void OnFullscreenToggled(bool isOn) {
            _screenService.SetFullscreen(isOn);
            _fullscreenValue.text = isOn ? "Yes" : "No";
        }
        private void OnBack() {
            _menuService.Show(MenuScreen.Settings);
        }
    }
}