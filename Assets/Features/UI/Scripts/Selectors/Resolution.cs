// autor: Murillo Gomes Yonamine
// data: 15/03/2026

using FifthSemester.Framework.UI;
using UnityEngine;

namespace FifthSemester.UI {
    public enum ResolutionOption {
        R320x240,
        R640x480,
        R800x600,
        R1024x768,
        R1280x720
    }
    public class Resolution : Selector<ResolutionOption> {
        private void Awake() {
            _items = new ResolutionOption[] {
                ResolutionOption.R320x240,
                ResolutionOption.R640x480,
                ResolutionOption.R800x600,
                ResolutionOption.R1024x768,
                ResolutionOption.R1280x720,
            };
        }
        protected override void OnItemSelected(ResolutionOption selectedItem) {
            bool isFullscreen = PlayerPrefs.GetInt("DisplayMode", 0) == (int)DisplayMode.Fullscreen;

            switch (selectedItem) {
                case ResolutionOption.R320x240:
                    Screen.SetResolution(320, 240, isFullscreen);
                    break;
                case ResolutionOption.R640x480:
                    Screen.SetResolution(640, 480, isFullscreen);
                    break;
                case ResolutionOption.R800x600:
                    Screen.SetResolution(800, 600, isFullscreen);
                    break;
                case ResolutionOption.R1024x768:
                    Screen.SetResolution(1024, 768, isFullscreen);
                    break;
                case ResolutionOption.R1280x720:
                    Screen.SetResolution(1280, 720, isFullscreen);
                    break;
            }

            OnSave();
        }

        protected override void OnSave() {
            PlayerPrefs.SetInt("Resolution", (int)_items[_currentIndex]);
            PlayerPrefs.Save();
        }
    }
}
