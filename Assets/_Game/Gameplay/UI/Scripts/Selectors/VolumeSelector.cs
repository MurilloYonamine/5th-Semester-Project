using FifthSemester.Framework.UI;
using TMPro;
using FifthSemester.Core.Services;
using UnityEngine;
using UnityEngine.UI;

namespace FifthSemester.UI {
    public enum CurrentVolume { V0 = 0, V25, V50, V75, V100 }

    public class VolumeSelector : Selector<CurrentVolume> {
        [SerializeField] protected TextMeshProUGUI _labelText;
        [SerializeField] private AudioType _audioType;
        [SerializeField] private GameObject[] _stepIcons;

        public enum AudioType { Master, Music, SFX }

        private IAudioService _audioService;

        protected void Awake() {
            _items = new CurrentVolume[] {
                CurrentVolume.V0, CurrentVolume.V25, CurrentVolume.V50, CurrentVolume.V75, CurrentVolume.V100
            };
        }
        protected override void Start() {
            base.Start();
            UpdateUI();
            _audioService = ServiceLocator.Get<IAudioService>();
        }

        protected override void OnItemSelected(CurrentVolume selectedItem) => ApplyToSystem(GetNormalizedVolume(selectedItem));

        private void ApplyToSystem(float volume) {

            switch (_audioType) {
                case AudioType.Master: _audioService.SetMasterVolume(volume); break;
                case AudioType.Music: _audioService.SetMusicVolume(volume); break;
                case AudioType.SFX: _audioService.SetSFXVolume(volume); break;
            }
        }

        protected override void OnSave() {
            PlayerPrefs.SetInt(_audioType + "Volume", (int)_items[_currentIndex]);
            PlayerPrefs.Save();
        }

        protected override void OnLoad() {
            _currentIndex = PlayerPrefs.GetInt(_audioType + "Volume", 2);
            ApplyToSystem(GetNormalizedVolume(_items[_currentIndex]));
        }

        protected override void UpdateUI() {
            if (_stepIcons == null || _stepIcons.Length == 0 || _items == null || _items.Length == 0)
                return;
            int active = _currentIndex;
            Color activeColor = _isFocused ? Color.yellow : Color.white;
            for (int i = 0; i < _stepIcons.Length; i++) {
                if (_stepIcons[i] == null) continue;
                _stepIcons[i].GetComponent<Image>().color = (i <= active) ? activeColor : Color.gray;
            }
        }

        protected override void UpdateVisualElements(Color targetColor) {
            if (_labelText != null) _labelText.color = targetColor;
            UpdateUI();
        }
        private float GetNormalizedVolume(CurrentVolume volume) => (int)volume * 0.25f;
    }
}
