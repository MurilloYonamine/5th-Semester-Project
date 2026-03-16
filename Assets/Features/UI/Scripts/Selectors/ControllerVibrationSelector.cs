// autor: Murillo Gomes Yonamine
// data: 15/03/2026

using FifthSemester.Framework.UI;
using UnityEngine;

namespace FifthSemester.UI
{
    public class ControllerVibrationSelector : TextSelector<ToggleState>
    {
        private void Awake() {
            _items = new ToggleState[] { ToggleState.Off, ToggleState.On };
        }

        protected override void OnItemSelected(ToggleState selectedItem) {
            OnSave();
        }

        protected override void OnSave() {
            PlayerPrefs.SetInt("ControllerVibration", (int)_items[_currentIndex]);
            PlayerPrefs.Save();
        }

        protected override void OnLoad() {
            int savedIndex = PlayerPrefs.GetInt("ControllerVibration", 1); // Default to On
            if (savedIndex >= 0 && savedIndex < _items.Length) {
                _currentIndex = savedIndex;
                OnItemSelected(_items[_currentIndex]);
            }
        }
    }
}
