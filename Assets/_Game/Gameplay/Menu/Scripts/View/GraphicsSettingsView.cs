using FifthSemester.Core.Enums;
using FifthSemester.Core.Services;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

namespace FifthSemester.Gameplay.Menu {
    public class GraphicsSettingsView : MenuViewBase {
        private ISettingsService _settingsService;
        private IGraphicsService _graphicsService;

        [Header("Defaults")]
        [SerializeField] private SettingsDefaultsShaders _defaultsShaders;

        [Header("Renderer Data")]
        [SerializeField] private UniversalRendererData _rendererData;

        [Header("Cameras")]
        [SerializeField] private Camera _mainCamera;
        [SerializeField] private Camera _graphicsCamera;

        [SerializeField] private Canvas _mainCanvas;
        [SerializeField] private Image _panelImage;

        [Header("Toggles")]
        [SerializeField] private Toggle _barrelDistortionToggle;
        [SerializeField] private Toggle _ditheringToggle;
        [SerializeField] private Toggle _pixelationToggle;
        [SerializeField] private Toggle _rollingBandsToggle;
        [SerializeField] private Toggle _scanlinesToggle;
        [SerializeField] private Toggle _vhsEffectToggle;

        [Header("TMP Values")]
        [SerializeField] private TextMeshProUGUI _barrelDistortionText;
        [SerializeField] private TextMeshProUGUI _ditheringText;
        [SerializeField] private TextMeshProUGUI _pixelationText;
        [SerializeField] private TextMeshProUGUI _rollingBandsText;
        [SerializeField] private TextMeshProUGUI _scanlinesText;
        [SerializeField] private TextMeshProUGUI _vhsEffectText;

        [Header("Buttons")]
        [SerializeField] private Button _backButton;
        [SerializeField] private Button _resetDefaultsButton;


        protected override MenuScreen MenuScreenType => MenuScreen.Settings_Graphics;

        protected override void Start() {
            base.Start();
            _settingsService = ServiceLocator.Get<ISettingsService>();
            _graphicsService = new GraphicsService(_rendererData);

            _barrelDistortionToggle.isOn = _settingsService.BarrelDistortion;
            _ditheringToggle.isOn = _settingsService.Dithering;
            _pixelationToggle.isOn = _settingsService.Pixelation;
            _rollingBandsToggle.isOn = _settingsService.RollingBands;
            _scanlinesToggle.isOn = _settingsService.Scanlines;
            _vhsEffectToggle.isOn = _settingsService.VHSEffect;

            OnToggleChanged(_settingsService.BarrelDistortion, _barrelDistortionText);
            OnToggleChanged(_settingsService.Dithering, _ditheringText);
            OnToggleChanged(_settingsService.Pixelation, _pixelationText);
            OnToggleChanged(_settingsService.RollingBands, _rollingBandsText);
            OnToggleChanged(_settingsService.Scanlines, _scanlinesText);
            OnToggleChanged(_settingsService.VHSEffect, _vhsEffectText);

            _barrelDistortionToggle.onValueChanged.AddListener(OnBarrelDistortionToggle);
            _ditheringToggle.onValueChanged.AddListener(OnDitheringToggle);
            _pixelationToggle.onValueChanged.AddListener(OnPixelationToggle);
            _rollingBandsToggle.onValueChanged.AddListener(OnRollingBandsToggle);
            _scanlinesToggle.onValueChanged.AddListener(OnScanlinesToggle);
            _vhsEffectToggle.onValueChanged.AddListener(OnVHSEffectToggle);

            _backButton.onClick.AddListener(OnBack);
            _resetDefaultsButton.onClick.AddListener(ResetToDefaults);
        }
        public void ResetToDefaults() {
            if (_defaultsShaders == null) return;
            _settingsService.BarrelDistortion = _defaultsShaders.BarrelDistortion;
            _settingsService.Dithering = _defaultsShaders.Dithering;
            _settingsService.Pixelation = _defaultsShaders.Pixelation;
            _settingsService.RollingBands = _defaultsShaders.RollingBands;
            _settingsService.Scanlines = _defaultsShaders.Scanlines;
            _settingsService.VHSEffect = _defaultsShaders.VHSEffect;

            _graphicsService?.SetBarrelDistortion(_defaultsShaders.BarrelDistortion);
            _graphicsService?.SetDithering(_defaultsShaders.Dithering);
            _graphicsService?.SetPixelation(_defaultsShaders.Pixelation);
            _graphicsService?.SetRollingBands(_defaultsShaders.RollingBands);
            _graphicsService?.SetScanlines(_defaultsShaders.Scanlines);
            _graphicsService?.SetVHSEffect(_defaultsShaders.VHSEffect);

            RefreshUI();
        }

        public override void OnShow() {
            base.OnShow();
            ChangeCamera(true);
        }

        public override void OnHide() {
            base.OnHide();
            ChangeCamera(false);
        }

        protected override void OnDestroy() {
            base.OnDestroy();
            _menuService?.Unregister(MenuScreen.Settings_Graphics);
        }

        public void OnBack() {
            _menuService.Show(MenuScreen.Settings);
        }

        private void ChangeCamera(bool value) {
            if (_mainCamera != null)
                _mainCamera.gameObject.SetActive(!value);

            if (_graphicsCamera != null)
                _graphicsCamera.gameObject.SetActive(value);

            _panelImage.enabled = !value;
            _mainCanvas.worldCamera = value ? _graphicsCamera : _mainCamera;
        }

        public void OnBarrelDistortionToggle(bool value) {
            _settingsService.BarrelDistortion = value;
            _graphicsService?.SetBarrelDistortion(value);
            OnToggleChanged(value, _barrelDistortionText);
        }

        public void OnDitheringToggle(bool value) {
            _settingsService.Dithering = value;
            _graphicsService?.SetDithering(value);
            OnToggleChanged(value, _ditheringText);
        }

        public void OnPixelationToggle(bool value) {
            _settingsService.Pixelation = value;
            _graphicsService?.SetPixelation(value);
            OnToggleChanged(value, _pixelationText);
        }

        public void OnRollingBandsToggle(bool value) {
            _settingsService.RollingBands = value;
            _graphicsService?.SetRollingBands(value);
            OnToggleChanged(value, _rollingBandsText);
        }

        public void OnScanlinesToggle(bool value) {
            _settingsService.Scanlines = value;
            _graphicsService?.SetScanlines(value);
            OnToggleChanged(value, _scanlinesText);
        }

        public void OnVHSEffectToggle(bool value) {
            _settingsService.VHSEffect = value;
            _graphicsService?.SetVHSEffect(value);
            OnToggleChanged(value, _vhsEffectText);
        }

        private void OnToggleChanged(bool value, TextMeshProUGUI text) {
            text.text = value ? "On" : "Off";
        }
        public void RefreshUI() {
            if (_settingsService == null) _settingsService = ServiceLocator.Get<ISettingsService>();
            _barrelDistortionToggle.isOn = _settingsService.BarrelDistortion;
            _ditheringToggle.isOn = _settingsService.Dithering;
            _pixelationToggle.isOn = _settingsService.Pixelation;
            _rollingBandsToggle.isOn = _settingsService.RollingBands;
            _scanlinesToggle.isOn = _settingsService.Scanlines;
            _vhsEffectToggle.isOn = _settingsService.VHSEffect;
            OnToggleChanged(_settingsService.BarrelDistortion, _barrelDistortionText);
            OnToggleChanged(_settingsService.Dithering, _ditheringText);
            OnToggleChanged(_settingsService.Pixelation, _pixelationText);
            OnToggleChanged(_settingsService.RollingBands, _rollingBandsText);
            OnToggleChanged(_settingsService.Scanlines, _scanlinesText);
            OnToggleChanged(_settingsService.VHSEffect, _vhsEffectText);
        }
    }
}
