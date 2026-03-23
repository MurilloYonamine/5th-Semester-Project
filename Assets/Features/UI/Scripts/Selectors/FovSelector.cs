// autor: Murillo Gomes Yonamine
// data: 15/03/2026

using FifthSemester.Framework.UI;
using UnityEngine;

namespace FifthSemester.UI
{
    public class FovSelector : TextSelector<int>
    {
        private int _defaultFov = 60;
        protected void Awake() {
            _items = new int[] { 60, 65, 70, 75, 80, 85, 90, 95, 100, 105, 110 };
        }

        protected override void OnItemSelected(int selectedItem) {
            OnSave();
        }

        protected override void OnSave() {
            PlayerPrefs.SetInt("FOV", _items[_currentIndex]);
            PlayerPrefs.Save();
        }

        protected override void OnLoad() {
            int savedFov = PlayerPrefs.GetInt("FOV", _defaultFov); // Default to 60
            for (int i = 0; i < _items.Length; i++) {
                if (_items[i] == savedFov) {
                    _currentIndex = i;
                    break;
                }
            }
            OnItemSelected(_items[_currentIndex]);
        }
    }
}
