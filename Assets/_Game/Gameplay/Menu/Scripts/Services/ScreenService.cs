using UnityEngine;
using FifthSemester.Core.Services;

namespace FifthSemester.Gameplay.Menu {
    public class ScreenService : IScreenService {
        
        public ScreenService() {
            ServiceLocator.Register<IScreenService>(this);
        }
        public void SetResolution(int width, int height, bool fullscreen) {
            FullScreenMode mode = fullscreen ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed;
            Screen.SetResolution(width, height, mode);
        }

        public void SetFullscreen(bool isFullscreen) {
            Screen.fullScreen = isFullscreen;
        }

        public void SetFrameRate(int frameRate) {
            Application.targetFrameRate = frameRate;
        }
    }
}