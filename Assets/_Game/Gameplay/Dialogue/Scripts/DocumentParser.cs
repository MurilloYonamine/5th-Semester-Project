// Autor: Murillo Gomes Yonamine
// Data: 23/05/2026

using System;
using System.Collections.Generic;
using UnityEngine;

namespace FifthSemester.Gameplay.Dialogue {
    public struct DocumentData {
        public string Title;
        public string[] Pages;
    }

    public static class DocumentParser {
        public static DocumentData Parse(TextAsset documentAsset) {
            DocumentData data = new DocumentData();
            data.Title = "Documento Desconhecido";
            data.Pages = new string[0];

            if (documentAsset == null || string.IsNullOrWhiteSpace(documentAsset.text)) {
                Debug.LogWarning("[DocumentParser] Ficheiro de texto vazio ou nulo!");
                return data;
            }

            // Lê o texto todo
            string rawText = documentAsset.text;

            string[] lines = rawText.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            if (lines.Length > 0 && lines[0].StartsWith("Title:")) {
                data.Title = lines[0].Replace("Title:", "").Trim();

                int firstLineEnd = rawText.IndexOf('\n');
                if (firstLineEnd >= 0) {
                    rawText = rawText.Substring(firstLineEnd).Trim();
                }
            }

            string[] rawPages = rawText.Split(new string[] { "---" }, StringSplitOptions.RemoveEmptyEntries);

            List<string> cleanPages = new List<string>();
            foreach (string page in rawPages) {
                string cleanPage = page.Trim();
                if (!string.IsNullOrEmpty(cleanPage)) {
                    cleanPages.Add(cleanPage);
                }
            }

            data.Pages = cleanPages.ToArray();
            return data;
        }
    }
}
