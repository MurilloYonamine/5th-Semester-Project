using UnityEngine;

namespace FifthSemester.Gameplay.Dialogue {
    public static class CaptionParser {
        public static string Parse(TextAsset captionAsset) {
            if (!TextAssetParserUtility.HasContent(captionAsset)) {
                return string.Empty;
            }

            return TextAssetParserUtility.NormalizeText(captionAsset.text);
        }
    }
}