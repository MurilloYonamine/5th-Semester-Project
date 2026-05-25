// autor: Murillo Gomes Yonamine
// data: 24/05/2026

using System;

namespace FifthSemester.Core.Services {
    public interface IFadeService {
        void FadeIn(float duration, Action onComplete = null);
        void FadeOut(float duration, Action onComplete = null);
    }
}