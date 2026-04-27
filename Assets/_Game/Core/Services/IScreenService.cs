namespace FifthSemester.Core.Services {
    public interface IScreenService {
        void SetResolution(int width, int height, bool fullscreen);
        void SetFullscreen(bool isFullscreen);
        void SetFrameRate(int frameRate);
    }
}