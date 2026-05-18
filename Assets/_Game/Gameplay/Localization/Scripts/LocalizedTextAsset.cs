// Autor: Murillo Gomes Yonamine
// Data: 18/05/2026

using System;
using UnityEngine;
using FifthSemester.Core.Enums;

namespace FifthSemester.Features.Localization {
    [Serializable]
    public struct LocalizedTextAsset {
        [Tooltip("Ficheiro TXT em Português")]
        public TextAsset Portuguese;

        [Tooltip("Ficheiro TXT em Inglês")]
        public TextAsset English;

        public TextAsset GetAsset(Language currentLanguage) {
            return currentLanguage switch {
                Language.English => English,
                Language.Portuguese => Portuguese,
                _ => Portuguese
            };
        }
    }
}
