using FifthSemester.Core.Enums;
using FifthSemester.Core.Services;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;
using UnityEngine.UI;

namespace FifthSemester.Gameplay.Menu {
    public class AudioSettingsView : MonoBehaviour {
        private IMenuService _menuService;
        private ISettingsService _settingsService;
        private IAudioService _audioService;

        [Header("Focus")]
        [SerializeField] private GameObject _focusFirstElement;

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
        private void Start() {
            _menuService = ServiceLocator.Get<IMenuService>();
            _settingsService = ServiceLocator.Get<ISettingsService>();
            _audioService = ServiceLocator.Get<IAudioService>();

            _menuService.Register(MenuScreen.Settings_Audio, gameObject);

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
        }
        private void OnEnable() {
            EventSystem.current.SetSelectedGameObject(null);

            if (_focusFirstElement != null) {
                EventSystem.current.SetSelectedGameObject(_focusFirstElement);
            }

            InputSystem.onAnyButtonPress.Call(OnAnyInput);
        }
        private void OnDestroy() {
            _menuService?.Unregister(MenuScreen.Settings_Audio);
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

        private void OnAnyInput(InputControl control) {
            if (control.device is Gamepad && EventSystem.current.currentSelectedGameObject == null) {
                EventSystem.current.SetSelectedGameObject(_focusFirstElement);
            }
        }
    }
}