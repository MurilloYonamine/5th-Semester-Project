// Autor: Murillo Gomes Yonamine
// Data: 20/05/2026

using FifthSemester.Core.Enums;

namespace FifthSemester.Core.Services {
    public interface ICutsceneService {
        void PlayCutscene(CutsceneType cutscene);
        void SkipActiveCutscene();
    }
}
