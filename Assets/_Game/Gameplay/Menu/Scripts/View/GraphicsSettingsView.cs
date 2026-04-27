using FifthSemester.Core.Enums;
using FifthSemester.Core.Services;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

namespace FifthSemester.Gameplay.Menu {
    public class GraphicsSettingsView : MonoBehaviour {
        private IMenuService _menuService;
        private ISettingsService _settingsService;
        private IGraphicsService _graphicsService;

        [Header("Renderer Data")]
        [SerializeField] private UniversalRendererData _rendererData;

        [Header("Focus")]
        [SerializeField] private GameObject _focusFirstElement;

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


        private void Start() {
            _menuService = ServiceLocator.Get<IMenuService>();
            _settingsService = ServiceLocator.Get<ISettingsService>();
            _graphicsService = new GraphicsService(_rendererData);

            _menuService.Register(MenuScreen.Settings_Graphics, gameObject);

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
        }

        private void OnEnable() {
            if (_focusFirstElement != null) {
                EventSystem.current.SetSelectedGameObject(_focusFirstElement);
            }
            ChangeCamera(true);
        }

        private void OnDisable() {
            ChangeCamera(false);
        }

        private void OnDestroy() {
            _menuService?.Unregister(MenuScreen.Settings_Graphics);
        }

        public void OnBack() {
            ChangeCamera(false);
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
    }
}