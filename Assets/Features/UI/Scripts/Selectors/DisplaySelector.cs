// autor: Murillo Gomes Yonamine
// data: 15/03/2026

using FifthSemester.Framework.UI;
using TMPro;
using UnityEngine;

namespace FifthSemester.UI {
    public enum DisplayMode {
        Fullscreen,
        Windowed,
        Borderless
    }
    public class DisplaySelector : TextSelector<DisplayMode> {
        protected void Awake() {
            _items = new DisplayMode[] {
                DisplayMode.Fullscreen,
                DisplayMode.Windowed,
                DisplayMode.Borderless
            };
        }
        protected override void OnItemSelected(DisplayMode selectedItem) {
            switch (selectedItem) {
                case DisplayMode.Fullscreen:
                    Screen.fullScreenMode = FullScreenMode.ExclusiveFullScreen;
                    break;
                case DisplayMode.Windowed:
                    Screen.fullScreenMode = FullScreenMode.Windowed;
                    break;
                case DisplayMode.Borderless:
                    Screen.fullScreenMode = FullScreenMode.FullScreenWindow;
                    break;
            }
            OnSave();
        }
        protected override void OnSave() {
            PlayerPrefs.SetInt("DisplayMode", (int)_items[_currentIndex]);
            PlayerPrefs.Save();
        }
        protected override void OnLoad() {
            int savedIndex = PlayerPrefs.GetInt("DisplayMode", 0);
            if (savedIndex >= 0 && savedIndex < _items.Length) {
                _currentIndex = savedIndex;
                OnItemSelected(_items[_currentIndex]);
            }
        }
    }
}
