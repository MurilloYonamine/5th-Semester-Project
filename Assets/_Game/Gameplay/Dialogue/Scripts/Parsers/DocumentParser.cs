// Autor: Murillo Gomes Yonamine
// Data: 23/05/2026

using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace FifthSemester.Gameplay {
    public struct DocumentData {
        public string Title;
        public string[] Pages;
    }

    public static class DocumentParser {
        public static DocumentData Parse(TextAsset documentAsset) {
            DocumentData data = new DocumentData {
                Title = "Documento Desconhecido",
                Pages = new string[0]
            };

            if (!TextAssetParserUtility.HasContent(documentAsset)) {
                Debug.LogWarning("[DocumentParser] Ficheiro de texto vazio ou nulo!");
                return data;
            }

            List<string> cleanPages = new List<string>();
            string[] lines = TextAssetParserUtility.SplitLines(documentAsset.text);
            int startIndex = 0;

            if (lines.Length > 0) {
                string firstLine = TextAssetParserUtility.NormalizeText(lines[0]);

                if (firstLine.StartsWith("Title:")) {
                    data.Title = firstLine.Replace("Title:", string.Empty).Trim();
                    startIndex = 1;
                }
            }

            StringBuilder pageBuilder = new StringBuilder();

            for (int i = startIndex; i < lines.Length; i++) {
                string line = lines[i];
                string trimmedLine = TextAssetParserUtility.NormalizeText(line);

                if (trimmedLine == "---") {
                    AddPage(pageBuilder, cleanPages);
                    continue;
                }

                if (pageBuilder.Length > 0) {
                    pageBuilder.AppendLine();
                }

                pageBuilder.Append(line);
            }

            AddPage(pageBuilder, cleanPages);

            data.Pages = cleanPages.ToArray();
            return data;
        }

        private static void AddPage(StringBuilder pageBuilder, List<string> pages) {
            if (pageBuilder.Length == 0) {
                return;
            }

            string cleanPage = pageBuilder.ToString().Trim();
            if (!string.IsNullOrEmpty(cleanPage)) {
                pages.Add(cleanPage);
            }

            pageBuilder.Clear();
        }
    }
}
