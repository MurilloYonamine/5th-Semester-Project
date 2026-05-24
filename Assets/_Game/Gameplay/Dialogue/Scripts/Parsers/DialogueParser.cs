// autor: Murillo Gomes Yonamine
// data: 13/05/2026

using System.Collections.Generic;
using UnityEngine;

namespace FifthSemester.Gameplay.Dialogue {
    public static class DialogueParser {
        public static Queue<ParsedDialogueLine> Parse(TextAsset file) {
            Queue<ParsedDialogueLine> parsedLines = new Queue<ParsedDialogueLine>();

            if (!TextAssetParserUtility.HasContent(file)) {
                return parsedLines;
            }

            string[] rawLines = TextAssetParserUtility.SplitLines(file.text);

            for (int i = 0; i < rawLines.Length; i++) {
                string line = TextAssetParserUtility.NormalizeText(rawLines[i]);
                if (string.IsNullOrWhiteSpace(line)) {
                    continue;
                }

                if (line.StartsWith("//")) {
                    continue;
                }

                int firstQuoteIndex = line.IndexOf('"');
                int lastQuoteIndex = line.LastIndexOf('"');

                if (firstQuoteIndex >= 0 && lastQuoteIndex > firstQuoteIndex) {
                    string speakerName = line.Substring(0, firstQuoteIndex).Replace(":", "").Trim();
                    string text = line.Substring(firstQuoteIndex + 1, lastQuoteIndex - firstQuoteIndex - 1);
                    text = text.Replace("\\\"", "\"");

                    parsedLines.Enqueue(new ParsedDialogueLine(speakerName, text));
                    continue;
                }

                parsedLines.Enqueue(new ParsedDialogueLine(string.Empty, line));
            }

            return parsedLines;
        }
    }
}
