// autor: Murillo Gomes Yonamine
// data: 13/05/2026

using System.Collections.Generic;
using UnityEngine;

namespace FifthSemester.Gameplay.Dialogue {
    public static class DialogueParser {
        public static Queue<ParsedDialogueLine> Parse(TextAsset file) {
            Queue<ParsedDialogueLine> parsedLines = new Queue<ParsedDialogueLine>();

            if (file == null || string.IsNullOrWhiteSpace(file.text)) {
                return parsedLines;
            }

            string[] rawLines = file.text.Split(new[] { "\r\n", "\n", "\r" }, System.StringSplitOptions.None);

            for (int i = 0; i < rawLines.Length; i++) {
                string line = rawLines[i];
                if (string.IsNullOrWhiteSpace(line)) {
                    continue;
                }

                string trimmedLine = line.Trim();
                if (trimmedLine.StartsWith("//")) {
                    continue;
                }

                int firstQuoteIndex = trimmedLine.IndexOf('"');
                int lastQuoteIndex = trimmedLine.LastIndexOf('"');

                if (firstQuoteIndex >= 0 && lastQuoteIndex > firstQuoteIndex) {
                    string speakerName = trimmedLine.Substring(0, firstQuoteIndex).Replace(":", "").Trim();
                    string text = trimmedLine.Substring(firstQuoteIndex + 1, lastQuoteIndex - firstQuoteIndex - 1);
                    text = text.Replace("\\\"", "\"");

                    parsedLines.Enqueue(new ParsedDialogueLine(speakerName, text));
                    continue;
                }

                parsedLines.Enqueue(new ParsedDialogueLine(string.Empty, trimmedLine));
            }

            return parsedLines;
        }
    }
}
