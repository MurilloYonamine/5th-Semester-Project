using UnityEngine;
using FifthSemester.Core.Services;

namespace FifthSemester.Gameplay.Menu {
    public class ScreenService : IScreenService {

        public ScreenService() {
            ServiceLocator.Register<IScreenService>(this);
        }
        public void SetResolution(int width, int height) {
            Screen.SetResolution(width, height, Screen.fullScreenMode);
        }

        public void SetFullscreen(bool isFullscreen) {
            FullScreenMode mode = isFullscreen ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed;
            Screen.fullScreenMode = mode;
            Screen.fullScreen = isFullscreen;
        }

        public void SetFrameRate(int frameRate) {
            Application.targetFrameRate = frameRate;
        }
    }
}