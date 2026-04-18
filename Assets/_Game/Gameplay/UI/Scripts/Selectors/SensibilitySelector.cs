using FifthSemester.Framework.UI;
using UnityEngine;

namespace FifthSemester.UI {
    public class SensibilitySelector : TextSelector<float> {
        private const string PREF_KEY = "MouseSensibility";

        protected void Awake() {
            _items = new float[50];
            for (int i = 0; i < _items.Length; i++) {
                _items[i] = (i + 1) * 0.1f;
            }
        }
        protected override void OnItemSelected(float selectedItem) {
            OnSave();
        }
        protected override void OnLoad() {
            if (_items == null || _items.Length == 0) return;

            float savedValue = PlayerPrefs.GetFloat(PREF_KEY, 1.0f);

            int index = Mathf.RoundToInt((savedValue - _items[0]) / 0.1f);

            _currentIndex = Mathf.Clamp(index, 0, _items.Length - 1);
        }

        protected override void OnSave() {
            PlayerPrefs.SetFloat(PREF_KEY, _items[_currentIndex]);
            PlayerPrefs.Save();
        }

        protected override void UpdateUI() {
            if (_optionText != null) {
                _optionText.text = _items[_currentIndex].ToString("F1");
            }
        }
    }
}
