// autor: Murillo Gomes Yonamine
// data: 24/05/2026

using UnityEngine;

namespace FifthSemester.Gameplay.Dialogue {
    internal static class TextAssetParserUtility {
        internal static bool HasContent(TextAsset textAsset) {
            return textAsset != null && !string.IsNullOrWhiteSpace(textAsset.text);
        }

        internal static string NormalizeText(string text) {
            if (string.IsNullOrEmpty(text)) {
                return string.Empty;
            }

            return text.TrimStart('\uFEFF').Trim();
        }

        internal static string[] SplitLines(string text) {
            return text.Split(new[] { "\r\n", "\n", "\r" }, System.StringSplitOptions.None);
        }
    }
}