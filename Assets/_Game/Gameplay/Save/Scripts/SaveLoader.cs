// Autor: Murillo Gomes Yonamine
// Data: 05/05/2026

using UnityEngine.SceneManagement;
using UnityEngine;
using FifthSemester.Core.Services;
using FifthSemester.Player;
using System.Collections;

namespace FifthSemester.Gameplay.Save {
    public static class SaveLoader {
        private static SaveData _pending;

        public static void SetPendingSave(SaveData data) {
            _pending = data;
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
            if (_pending == null) {
                SceneManager.sceneLoaded -= OnSceneLoaded;
                return;
            }

            Object.FindFirstObjectByType<MonoBehaviour>()?.StartCoroutine(ApplySaveDelayed());
        }

        private static IEnumerator ApplySaveDelayed() {
            yield return null;
            
            SavePoint[] allPoints = Object.FindObjectsByType<SavePoint>(FindObjectsSortMode.None);
            for (int i = 0; i < allPoints.Length; i++) {
                Debug.Log($"  [{i}] ID='{allPoints[i].Id}'");
            }

            SavePoint target = GetSavePoint(_pending.LastCheckpointId);
            PlayerController player = Object.FindFirstObjectByType<PlayerController>();

            if (target != null && player != null) {
                target.SetPlayerController(player);
                target.LoadGame(_pending);
            } 

            _pending = null;
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private static SavePoint GetSavePoint(string checkpointId) {
            SavePoint[] savePoints = Object.FindObjectsByType<SavePoint>(FindObjectsSortMode.None);

            for (int index = 0; index < savePoints.Length; index++) {
                if (savePoints[index].Id == checkpointId) {
                    return savePoints[index];
                }
            }

            return null;
        }
    }
}
