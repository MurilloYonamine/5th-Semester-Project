// Autor: Murillo Gomes Yonamine
// Data: 18/05/2026

#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace FifthSemester.EditorTools.Dialogue {
    public class DialogueValidator : EditorWindow {
        [MenuItem("Dialogue/Validar Diálogos (.txt)")]
        public static void ValidateDialogues() {
            string[] guids = AssetDatabase.FindAssets("t:TextAsset", new[] { "Assets/_Game/Data/Dialogue" });
            int errorCount = 0;
            int warningCount = 0;
            int filesChecked = 0;

            Debug.Log("<b>[Validador de Diálogos]</b> Iniciando varredura...");

            foreach (string guid in guids) {
                string path = AssetDatabase.GUIDToAssetPath(guid);

                if (!path.EndsWith(".txt")) continue;

                // Filtra para analisar apenas arquivos que estejam em pastas com "Dialogue" no nome
                if (!path.Contains("Dialogue")) continue;

                TextAsset file = AssetDatabase.LoadAssetAtPath<TextAsset>(path);
                if (file == null) continue;

                filesChecked++;
                string[] lines = file.text.Split(new[] { "\r\n", "\n", "\r" }, System.StringSplitOptions.None);

                for (int i = 0; i < lines.Length; i++) {
                    string line = lines[i];
                    if (string.IsNullOrWhiteSpace(line)) continue;

                    string trimmedLine = line.Trim();
                    if (trimmedLine.StartsWith("//")) continue;

                    int firstQuote = trimmedLine.IndexOf('"');
                    int lastQuote = trimmedLine.LastIndexOf('"');

                    string linkText = $"<a href=\"{path}\" line=\"{i + 1}\"><b>{file.name} (Linha {i + 1})</b></a>";

                    // ERRO 1: Apenas uma aspa na linha (abriu mas não fechou, ou fechou mas não abriu)
                    if (firstQuote >= 0 && lastQuote == firstQuote) {
                        Debug.LogError($"[ERRO] Arquivo: {linkText} | Aspas não fechadas.\nConteúdo: {trimmedLine}", file);
                        errorCount++;
                        continue;
                    }

                    // AVISO 1: Tem nome de personagem, mas esqueceu os dois pontos ":"
                    if (firstQuote > 0) {
                        string speakerPart = trimmedLine.Substring(0, firstQuote);
                        if (!speakerPart.Contains(":")) {
                            Debug.LogWarning($"[AVISO] Arquivo: {linkText} | Faltou os dois pontos ':' depois do nome.\nConteúdo: {trimmedLine}", file);
                            warningCount++;
                        }
                    }

                    // AVISO 2: Texto sobrando depois da última aspa (pode ser um erro de digitação)
                    if (lastQuote > 0 && lastQuote < trimmedLine.Length - 1) {
                        Debug.LogWarning($"[AVISO] Arquivo: {linkText} | Existe texto fora das aspas no final.\nConteúdo: {trimmedLine}", file);
                        warningCount++;
                    }

                    // AVISO 3: Linha sem aspas (O Parser aceita isso como narrador, mas é bom avisar para ter certeza)
                    if (firstQuote == -1 && trimmedLine.Contains(":")) {
                        Debug.LogWarning($"[AVISO] Arquivo: {linkText} | Linha tem um ':' mas não tem aspas. Intencional?\nConteúdo: {trimmedLine}", file);
                        warningCount++;
                    }
                }
            }

            // Relatório Final
            if (errorCount == 0 && warningCount == 0) {
                Debug.Log($"<color=green><b>[Validador de Diálogos]</b> Sucesso! {filesChecked} arquivos analisados e nenhuma falha de formatação encontrada.</color>");
            }
            else {
                Debug.Log($"<color=orange><b>[Validador de Diálogos]</b> Varredura concluída em {filesChecked} arquivos: {errorCount} Erros | {warningCount} Avisos. Verifique os logs acima.</color>");
            }
        }
    }
}
#endif
