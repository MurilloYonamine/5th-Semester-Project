using System.Collections.Generic;
using FifthSemester.Core.Enums;
using FifthSemester.Core.Services;
using FifthSemester.Framework.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FifthSemester.Gameplay.Menu {

    public class GameplaySettingsView : MenuViewBase {
        private ISettingsService _settingsService;

        [Header("Defaults")]
        [SerializeField] private SettingsDefaultsGameplay _defaultsGameplay;

        [SerializeField] private Button _backButton;
        [SerializeField] private Button _resetDefaultsButton;

        [Header("Invert Y-Axis")]
        [SerializeField] private Toggle _invertYAxisToggle;
        [SerializeField] private TextMeshProUGUI _invertYAxisLabel;

        [Header("Sensibility")]
        [SerializeField] private Slider _sensibilitySlider;
        [SerializeField] private TextMeshProUGUI _sensibilityValueText;

        [Header("Language")]
        [SerializeField] private OptionSelector _language;

        protected override MenuScreen MenuScreenType => MenuScreen.Settings_Gameplay;

        protected override void Start() {
            base.Start();
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
            _resetDefaultsButton.onClick.AddListener(ResetToDefaults);
        }
        public void ResetToDefaults() {
            if (_defaultsGameplay == null) return;
            _settingsService.Language = _defaultsGameplay.Language;
            _settingsService.InvertYAxis = _defaultsGameplay.InvertYAxis;
            _settingsService.Sensibility = _defaultsGameplay.Sensibility;
            RefreshUI();
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
        public void RefreshUI() {
            if (_settingsService == null) _settingsService = ServiceLocator.Get<ISettingsService>();
            _invertYAxisToggle.isOn = _settingsService.InvertYAxis;
            _invertYAxisLabel.text = _settingsService.InvertYAxis ? "Yes" : "No";
            _sensibilitySlider.value = _settingsService.Sensibility;
            _sensibilityValueText.text = _settingsService.Sensibility.ToString("F2");
            _language.SetValue((int)_settingsService.Language);
        }
    }
}