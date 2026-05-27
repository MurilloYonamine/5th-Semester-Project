using FifthSemester.Core.Enums;
using FifthSemester.Core.Services;
using FifthSemester.Features.Localization;
using FifthSemester.Framework.UI;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FifthSemester.Gameplay.Menu {

    public class GameplaySettingsView : MenuViewBase {
        private ISettingsService _settingsService;
        private IGameplayService _gameplayService;
        private ILocalizationService _localizationService;

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

        protected override void Awake() {
            base.Awake();

            _settingsService = ServiceLocator.Get<ISettingsService>();
            _localizationService = ServiceLocator.Get<ILocalizationService>(); // Novo!
            _gameplayService = new GameplayService(_settingsService);
        }

        protected override void Start() {
            base.Start();

            _backButton.onClick.AddListener(OnBack);
            _resetDefaultsButton.onClick.AddListener(ResetToDefaults);

            _invertYAxisToggle.onValueChanged.AddListener(OnInvertYAxisChanged);
            _sensibilitySlider.onValueChanged.AddListener(OnSensibilityChanged);

            _language.Initialize(GetLanguageOptions(), (int)_settingsService.Language);
            _language.OnValueChanged += OnLanguageChanged;

            RefreshUI();
        }

        public void ResetToDefaults() {
            if (_defaultsGameplay == null) return;

            PlayMenuSecondarySfx();
            _settingsService.Language = _defaultsGameplay.Language;
            _settingsService.InvertYAxis = _defaultsGameplay.InvertYAxis;
            _settingsService.Sensibility = _defaultsGameplay.Sensibility;
            RefreshUI();
        }

        public void OnBack() {
            PlayMenuBackSfx();
            _menuService.Show(MenuScreen.Settings);
        }

        public void OnInvertYAxisChanged(bool isInverted) {
            _settingsService.InvertYAxis = isInverted;
            UpdateInvertYAxisLabel(isInverted);
        }

        public void OnSensibilityChanged(float value) {
            _settingsService.Sensibility = value;
            _sensibilityValueText.text = value.ToString("F2");
        }

        public void OnLanguageChanged(int index) {
            _settingsService.Language = (Language)index;
            UpdateInvertYAxisLabel(_settingsService.InvertYAxis);
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

            UpdateInvertYAxisLabel(_settingsService.InvertYAxis);

            _sensibilitySlider.value = _settingsService.Sensibility;
            _sensibilityValueText.text = _settingsService.Sensibility.ToString("F2");

            _language.SetValue((int)_settingsService.Language);
        }

        private void UpdateInvertYAxisLabel(bool isInverted) {
            if (_localizationService == null) return;

            string key = isInverted ? "general_yes" : "general_no";
            _invertYAxisLabel.text = _localizationService.GetText(key);
        }
    }
}
