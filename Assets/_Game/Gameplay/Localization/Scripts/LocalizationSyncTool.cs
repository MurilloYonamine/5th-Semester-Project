// Autor: Murillo Gomes Yonamine
// Data: 17/05/2026

#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.Networking;
using System.IO;

namespace FifthSemester.Features.Localization.EditorTools {
    public class LocalizationSyncTool : EditorWindow {
        // O ID extraído do teu link
        private const string SheetId = "1szeeod-nQVArgDkAGl3sB9kFgaTGmjD-hgcg8V0WaiM";

        // A primeira aba da planilha
        private const string Gid = "0";

        private const string SavePath = "Assets/_Game/Data/Localization/LocalizedText.csv";

        [MenuItem("Localization/Sincronizar Google Sheets")]
        public static void SyncData() {
            Debug.Log("[Localization] A transferir dados do Google Sheets...");

            // Link do Google que converte a planilha pública num ficheiro CSV
            string url = $"https://docs.google.com/spreadsheets/d/{SheetId}/export?format=csv&gid={Gid}";

            var request = UnityWebRequest.Get(url);
            var operation = request.SendWebRequest();

            // Espera que o download termine (O while é seguro aqui pois estamos no fluxo do Editor, não em Gameplay)
            while (!operation.isDone) { }

            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError) {
                Debug.LogError($"[Localization] Erro de conexão: {request.error}");
            }
            else {
                bool fileExists = File.Exists(SavePath);

                // Garante que a pasta existe antes de tentar guardar o ficheiro
                FileInfo fileInfo = new FileInfo(SavePath);
                fileInfo.Directory?.Create();

                // Guarda o texto recebido num ficheiro CSV
                File.WriteAllText(SavePath, request.downloadHandler.text);

                // Força a Unity a atualizar a janela do Project para mostrar o ficheiro novo
                AssetDatabase.Refresh();

                if (fileExists) {
                    Debug.Log($"[Localization] Sucesso! O arquivo foi atualizado em {SavePath}");
                }
                else {
                    Debug.Log($"[Localization] Sucesso! Novo arquivo criado em {SavePath}");
                }
            }
        }
    }
}
#endif
