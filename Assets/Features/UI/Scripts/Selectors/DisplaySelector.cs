// autor: Murillo Gomes Yonamine
// data: 15/03/2026

using FifthSemester.Framework.UI;
using UnityEngine;

namespace FifthSemester.UI {
    public enum DisplayMode {
        Fullscreen,
        Windowed,
        Borderless
    }
    public class DisplaySelector : Selector<DisplayMode> {

        private void Awake() {
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
    }
}
