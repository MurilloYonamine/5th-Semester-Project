namespace FifthSemester.Core.Services {
    public interface IGraphicsService {
        void SetBarrelDistortion(bool isEnabled);
        void SetDithering(bool isEnabled);
        void SetPixelation(bool isEnabled);
        void SetRollingBands(bool isEnabled);
        void SetScanlines(bool isEnabled);
        void SetVHSEffect(bool isEnabled);
    }
}