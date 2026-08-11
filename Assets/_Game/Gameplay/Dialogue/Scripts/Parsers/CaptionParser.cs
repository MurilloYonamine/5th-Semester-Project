using System.Collections.Generic;
using UnityEngine;

namespace FifthSemester.Gameplay {
    public static class CaptionParser {
        public static string Parse(TextAsset captionAsset) {
            if (!TextAssetParserUtility.HasContent(captionAsset)) {
                return string.Empty;
            }

            string[] rawLines = TextAssetParserUtility.SplitLines(captionAsset.text);
            List<string> cleanedLines = new List<string>();

            for (int i = 0; i < rawLines.Length; i++) {
                string line = TextAssetParserUtility.NormalizeText(rawLines[i]);

                if (string.IsNullOrWhiteSpace(line)) {
                    continue;
                }

                if (line.StartsWith("//")) {
                    continue;
                }

                cleanedLines.Add(StripQuotes(line));
            }

            return string.Join("\n", cleanedLines);
        }

        private static string StripQuotes(string line) {
            if (line.Length >= 2 && line.StartsWith("\"") && line.EndsWith("\"")) {
                return line.Substring(1, line.Length - 2).Trim();
            }

            return line;
        }
    }
}