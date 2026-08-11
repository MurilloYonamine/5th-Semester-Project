using System.Collections.Generic;
using FifthSemester.Core.Enums;
using FifthSemester.Core.Services;
using FifthSemester.Framework;
using UnityEngine;
using UnityEngine.UI;

using TMPro;

namespace FifthSemester.UI {
    public class ScreenSettingsView : MenuViewBase {
        private ISettingsService _settingsService;
        private IScreenService _screenService;
        private ILocalizationService _localizationService;

        [Header("Defaults")]
        [SerializeField] private SettingsDefaultsScreen _defaultsScreen;

        [Header("Selectors")]
        [SerializeField] private OptionSelector _resolutionSelector;
        [SerializeField] private OptionSelector _fpsSelector;
        [SerializeField] private Toggle _fullscreenToggle;

        [Header("Buttons")]
        [SerializeField] private Button _backButton;
        [SerializeField] private Button _resetDefaultsButton;

        [Header("TMP Values")]
        [SerializeField] private TextMeshProUGUI _fullscreenValue;
        private readonly List<int> _fpsValues = new() { 24, 30, -1 };

        protected override MenuScreen MenuScreenType => MenuScreen.Settings_Screen;

        protected override void Awake() {
            base.Awake();

            _settingsService = ServiceLocator.Get<ISettingsService>();
            _localizationService = ServiceLocator.Get<ILocalizationService>();
            _screenService = new ScreenService();
        }

        protected override void Start() {
            base.Start();

            _fullscreenToggle.onValueChanged.AddListener(OnFullscreenToggled);

            _resolutionSelector.OnValueChanged += OnResolutionChanged;
            _fpsSelector.OnValueChanged += OnFPSChanged;

            _backButton.onClick.AddListener(OnBack);
            _resetDefaultsButton.onClick.AddListener(ResetToDefaults);

            RefreshUI();
        }

        public override void OnShow() {
            base.OnShow();
            RefreshUI(); 
        }

        public void ResetToDefaults() {
            if (_defaultsScreen == null) return;

            PlayMenuSecondarySfx();

            _settingsService.FrameRate = _defaultsScreen.FrameRate;
            _settingsService.IsFullscreen = _defaultsScreen.IsFullscreen;
            _settingsService.ResolutionIndex = _defaultsScreen.ResolutionIndex;

            _screenService.SetResolution(
                _settingsService.AvailableResolutions[_settingsService.ResolutionIndex].x,
                _settingsService.AvailableResolutions[_settingsService.ResolutionIndex].y
            );

            _screenService.SetFrameRate(_settingsService.FrameRate);
            _screenService.SetFullscreen(_settingsService.IsFullscreen);

            RefreshUI();
        }

        public void OnResolutionChanged(int index) {
            var resolution = _settingsService.AvailableResolutions[index];
            _settingsService.ResolutionIndex = index;
            _screenService.SetResolution(resolution.x, resolution.y);
        }

        public void OnFPSChanged(int index) {
            int fps = _fpsValues[index];
            _screenService.SetFrameRate(fps);
        }

        private List<string> GetFPSOptions() {
            List<string> options = new();
            foreach (var fps in _fpsValues) {
                if (fps > 0) {
                    options.Add(fps.ToString());
                }
                else {
                    string unlimitedText = _localizationService != null ? _localizationService.GetText("settings_screen_unlimited") : "Unlimited";
                    options.Add(unlimitedText);
                }
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
            UpdateFullscreenLabel(isOn);
        }

        private void UpdateFullscreenLabel(bool isOn) {
            if (_localizationService == null) return;
            string key = isOn ? "general_yes" : "general_no";
            _fullscreenValue.text = _localizationService.GetText(key);
        }

        private void OnBack() {
            PlayMenuBackSfx();
            _menuService.Show(MenuScreen.Settings);
        }

        public void RefreshUI() {
            if (_settingsService == null) _settingsService = ServiceLocator.Get<ISettingsService>();

            _fullscreenToggle.isOn = _settingsService.IsFullscreen;
            UpdateFullscreenLabel(_settingsService.IsFullscreen);

            _resolutionSelector.Initialize(GetResolutionOptions(), _settingsService.ResolutionIndex);

            int fpsIdx = _fpsValues.IndexOf(_settingsService.FrameRate);
            if (fpsIdx < 0) fpsIdx = 0;
            _fpsSelector.Initialize(GetFPSOptions(), fpsIdx);
        }
    }
}
