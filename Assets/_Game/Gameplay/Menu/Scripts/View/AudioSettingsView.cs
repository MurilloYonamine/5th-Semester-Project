using FifthSemester.Core.Enums;
using FifthSemester.Core.Services;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FifthSemester.Gameplay.Menu {
    public class AudioSettingsView : MenuViewBase {
        private ISettingsService _settingsService;
        private IAudioService _audioService;

        [Header("Defaults")]
        [SerializeField] private SettingsDefaultsAudio _defaultsAudio;

        [Header("Sliders")]
        [SerializeField] private Slider _masterVolumeSlider;
        [SerializeField] private Slider _musicVolumeSlider;
        [SerializeField] private Slider _sfxVolumeSlider;
        [SerializeField] private Slider _ambienceVolumeSlider;

        [Header("TMP Values")]
        [SerializeField] private TextMeshProUGUI _masterVolumeText;
        [SerializeField] private TextMeshProUGUI _musicVolumeText;
        [SerializeField] private TextMeshProUGUI _sfxVolumeText;
        [SerializeField] private TextMeshProUGUI _ambienceVolumeText;
        [SerializeField] private TextMeshProUGUI _forceMonoText;

        [SerializeField] private Toggle _forceMonoToggle;
        [SerializeField] private Button _backButton;
        [SerializeField] private Button _resetDefaultsButton;
        protected override MenuScreen MenuScreenType => MenuScreen.Settings_Audio;

        protected override void Start() {
            base.Start();
            _settingsService = ServiceLocator.Get<ISettingsService>();
            _audioService = ServiceLocator.Get<IAudioService>();

            _masterVolumeSlider.value = _settingsService.MasterVolume;
            _masterVolumeText.text = Mathf.RoundToInt(_settingsService.MasterVolume).ToString();

            _musicVolumeSlider.value = _settingsService.MusicVolume;
            _musicVolumeText.text = Mathf.RoundToInt(_settingsService.MusicVolume).ToString();

            _sfxVolumeSlider.value = _settingsService.SFXVolume;
            _sfxVolumeText.text = Mathf.RoundToInt(_settingsService.SFXVolume).ToString();

            _ambienceVolumeSlider.value = _settingsService.AmbienceVolume;
            _ambienceVolumeText.text = Mathf.RoundToInt(_settingsService.AmbienceVolume).ToString();

            _forceMonoToggle.isOn = _settingsService.ForceMonoAudio;
            _forceMonoText.text = _settingsService.ForceMonoAudio ? "Yes" : "No";

            _masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
            _musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
            _sfxVolumeSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
            _ambienceVolumeSlider.onValueChanged.AddListener(OnAmbienceVolumeChanged);
            _forceMonoToggle.onValueChanged.AddListener(OnForceMonoAudioChanged);

            _backButton.onClick.AddListener(OnBack);
            _resetDefaultsButton.onClick.AddListener(ResetToDefaults);
        }
        public void ResetToDefaults() {
            if (_defaultsAudio == null) return;
            _settingsService.MasterVolume = _defaultsAudio.MasterVolume;
            _settingsService.MusicVolume = _defaultsAudio.MusicVolume;
            _settingsService.SFXVolume = _defaultsAudio.SFXVolume;
            _settingsService.AmbienceVolume = _defaultsAudio.AmbienceVolume;
            _settingsService.ForceMonoAudio = _defaultsAudio.ForceMonoAudio;

            _audioService?.SetMasterVolume(_defaultsAudio.MasterVolume);
            _audioService?.SetMusicVolume(_defaultsAudio.MusicVolume);
            _audioService?.SetSFXVolume(_defaultsAudio.SFXVolume);
            _audioService?.SetAmbienceVolume(_defaultsAudio.AmbienceVolume);

            RefreshUI();
        }
        public void OnBack() {
            _menuService.Show(MenuScreen.Settings);
        }

        public void OnMasterVolumeChanged(float value) {
            _settingsService.MasterVolume = value;
            _audioService?.SetMasterVolume(value);
            _masterVolumeText.text = Mathf.RoundToInt(value).ToString();
        }

        public void OnMusicVolumeChanged(float value) {
            _settingsService.MusicVolume = value;
            _audioService?.SetMusicVolume(value);
            _musicVolumeText.text = Mathf.RoundToInt(value).ToString();
        }
        public void OnSFXVolumeChanged(float value) {
            _settingsService.SFXVolume = value;
            _audioService?.SetSFXVolume(value);
            _sfxVolumeText.text = Mathf.RoundToInt(value).ToString();
        }
        public void OnAmbienceVolumeChanged(float value) {
            _settingsService.AmbienceVolume = value;
            _audioService?.SetAmbienceVolume(value);
            _ambienceVolumeText.text = Mathf.RoundToInt(value).ToString();
        }
        public void OnForceMonoAudioChanged(bool value) {
            _settingsService.ForceMonoAudio = value;
            _forceMonoText.text = value ? "Yes" : "No";
        }

        public void RefreshUI() {
            if (_settingsService == null) _settingsService = ServiceLocator.Get<ISettingsService>();
            _masterVolumeSlider.value = _settingsService.MasterVolume;
            _masterVolumeText.text = Mathf.RoundToInt(_settingsService.MasterVolume).ToString();
            _musicVolumeSlider.value = _settingsService.MusicVolume;
            _musicVolumeText.text = Mathf.RoundToInt(_settingsService.MusicVolume).ToString();
            _sfxVolumeSlider.value = _settingsService.SFXVolume;
            _sfxVolumeText.text = Mathf.RoundToInt(_settingsService.SFXVolume).ToString();
            _ambienceVolumeSlider.value = _settingsService.AmbienceVolume;
            _ambienceVolumeText.text = Mathf.RoundToInt(_settingsService.AmbienceVolume).ToString();
            _forceMonoToggle.isOn = _settingsService.ForceMonoAudio;
            _forceMonoText.text = _settingsService.ForceMonoAudio ? "Yes" : "No";
        }
    }
}