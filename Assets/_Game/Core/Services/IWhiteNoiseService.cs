// Autor: Murillo Gomes Yonamine
// Data: 09/05/2026

namespace FifthSemester.Core.Services {
    public interface IWhiteNoiseService {
        void RequestIntensity(float intensity);
        void ResetIntensity();
    }
}