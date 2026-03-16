// autor: Murillo Gomes Yonamine
// data: 15/03/2026

using FifthSemester.Framework.UI;
using FifthSemester.Shared.AudioSystem;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FifthSemester.UI {
    public enum CurrentVolume {
        V0, V25, V50, V75, V100
    }
    public class VolumeSelector : Selector<CurrentVolume> {
        [Header("Text References")]
        [SerializeField] protected TextMeshProUGUI _labelText;

        [Header("Audio Settings")]
        [SerializeField] private AudioType _audioType;
        public enum AudioType { Master, Music, SFX }

        [Header("Volume Visuals")]
        [SerializeField] private Color _filledUnfocusedColor = Color.white; // Cor de quando está preenchido, mas sem foco

        [SerializeField] private GameObject[] _stepIcons;
        private void Awake() {
            _items = new CurrentVolume[] {
                CurrentVolume.V0,
                CurrentVolume.V25,
                CurrentVolume.V50,
                CurrentVolume.V75,
                CurrentVolume.V100
            };
        }
        protected override void OnItemSelected(CurrentVolume selectedItem) {
            float volumeValue = GetNormalizedVolume(selectedItem);

            switch (_audioType) {
                case AudioType.Master:
                    AudioManager.Instance.SetMasterVolume(volumeValue);
                    break;
                case AudioType.Music:
                    AudioManager.Instance.SetMusicVolume(volumeValue);
                    break;
                case AudioType.SFX:
                    AudioManager.Instance.SetSFXVolume(volumeValue);
                    break;
            }

            OnSave();
        }
        protected override void OnSave() {
            string prefKey = _audioType.ToString() + "Volume";
            PlayerPrefs.SetInt(prefKey, (int)_items[_currentIndex]);
            PlayerPrefs.Save();
        }
        protected override void OnLoad() {
            string prefKey = _audioType.ToString() + "Volume";
            int savedIndex = PlayerPrefs.GetInt(prefKey, 0);
            if (savedIndex >= 0 && savedIndex < _items.Length) {
                _currentIndex = savedIndex;
                OnItemSelected(_items[_currentIndex]);
            }
        }
        protected override void UpdateUI() {
            Color filledColor = _isFocused ? _focusedColor : _filledUnfocusedColor;
            UpdateIconsColor(filledColor);
        }

        protected override void OnHighlight(bool highlight) {
            if (_isFocused) return;

            Color targetColor = highlight ? _hoverColor : _unfocusedColor;
            _labelText.color = targetColor;

            Color filledColor = highlight ? _hoverColor : _filledUnfocusedColor;
            UpdateIconsColor(filledColor);
        }

        protected override void UpdateVisualState() {
            Color filledColor = _isFocused ? _focusedColor : _filledUnfocusedColor;
            UpdateIconsColor(filledColor);
        }

        private void UpdateIconsColor(Color activeColor) {
            if (_stepIcons == null) return;

            for (int i = 0; i < _stepIcons.Length; i++) {
                if (_stepIcons[i] == null) continue;

                Image image = _stepIcons[i].GetComponent<Image>();

                if (i <= _currentIndex) {
                    image.color = activeColor;
                }
                else {
                    image.color = _unfocusedColor;
                }
            }
        }
        private float GetNormalizedVolume(CurrentVolume volumeEnum) {
            float volumeValue = 0f;
            switch (volumeEnum) {
                case CurrentVolume.V0:
                    volumeValue = 0f;
                    break;
                case CurrentVolume.V25:
                    volumeValue = 0.25f;
                    break;
                case CurrentVolume.V50:
                    volumeValue = 0.5f;
                    break;
                case CurrentVolume.V75:
                    volumeValue = 0.75f;
                    break;
                case CurrentVolume.V100:
                    volumeValue = 1f;
                    break;
            }
            return volumeValue;
        }
    }
}
