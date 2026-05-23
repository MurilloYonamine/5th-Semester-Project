// Autor: Murillo Gomes Yonamine
// Data: 17/05/2026

using FifthSemester.Core.Enums;
using FifthSemester.Core.Services;

namespace FifthSemester.Core.Services {
    public interface ILocalizationService {
        void SetLanguage(Language language);
        string GetText(string key);
    }
}
