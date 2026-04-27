using System.Collections.Generic;
using FifthSemester.Core.Enums;
using FifthSemester.Core.Services;
using FifthSemester.Framework.UI;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;
using UnityEngine.UI;

namespace FifthSemester.Gameplay.Menu {
    public class GameplaySettingsView : MonoBehaviour {
        private IMenuService _menuService;
        private ISettingsService _settingsService;

        [Header("Focus")]
        [SerializeField] private GameObject _focusFirstElement;

        [SerializeField] private Button _backButton;

        [Header("Invert Y-Axis")]
        [SerializeField] private Toggle _invertYAxisToggle;
        [SerializeField] private TextMeshProUGUI _invertYAxisLabel;

        [Header("Sensibility")]
        [SerializeField] private Slider _sensibilitySlider;
        [SerializeField] private TextMeshProUGUI _sensibilityValueText;

        [Header("Language")]
        [SerializeField] private OptionSelector _language;

        private void Start() {
            _menuService = ServiceLocator.Get<IMenuService>();
            _settingsService = ServiceLocator.Get<ISettingsService>();

            _backButton.onClick.AddListener(OnBack);

            _invertYAxisToggle.isOn = _settingsService.InvertYAxis;
            _invertYAxisToggle.onValueChanged.AddListener(OnInvertYAxisChanged);
            _invertYAxisLabel.text = _settingsService.InvertYAxis ? "Yes" : "No";

            _sensibilitySlider.onValueChanged.AddListener(OnSensibilityChanged);
            _sensibilitySlider.value = _settingsService.Sensibility;
            _sensibilityValueText.text = _settingsService.Sensibility.ToString("F2");

            _language.Initialize(GetLanguageOptions(), (int)_settingsService.Language);
            _language.OnValueChanged += OnLanguageChanged;

            _menuService.Register(MenuScreen.Settings_Gameplay, gameObject);
        }

        private void OnEnable() {
            EventSystem.current.SetSelectedGameObject(null);

            if (_focusFirstElement != null) {
                EventSystem.current.SetSelectedGameObject(_focusFirstElement);
            }

            InputSystem.onAnyButtonPress.Call(OnAnyInput);
        }
        private void OnDestroy() {
            _menuService?.Unregister(MenuScreen.Settings_Gameplay);
        }
        public void OnBack() {
            _menuService.Show(MenuScreen.Settings);
        }
        public void OnInvertYAxisChanged(bool isInverted) {
            _settingsService.InvertYAxis = isInverted;
            _invertYAxisLabel.text = isInverted ? "Yes" : "No";
        }

        public void OnSensibilityChanged(float value) {
            _settingsService.Sensibility = value;
            _sensibilityValueText.text = value.ToString("F2");
        }
        public void OnLanguageChanged(int index) {
            _settingsService.Language = (Language)index;
        }
        private List<string> GetLanguageOptions() {
            var options = new List<string>();
            foreach (Language lang in System.Enum.GetValues(typeof(Language))) {
                options.Add(lang.ToString());
            }
            return options;
        }
        private void OnAnyInput(InputControl control) {
            if (control.device is Gamepad && EventSystem.current.currentSelectedGameObject == null) {
                EventSystem.current.SetSelectedGameObject(_focusFirstElement);
            }
        }
    }
}