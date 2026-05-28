// Autor: Murillo Gomes Yonamine
// Data: 05/05/2026

using UnityEngine;
using System;
using System.Collections.Generic;
using FifthSemester.Core.Services;

namespace FifthSemester.Gameplay.Save{
    public class SaveService : ISaveService {
        private const string SAVE_PREFIX = "save_";
        private const string AUTOSAVE_SLOT = "default";

        public event Action<string> OnSaveCompleted;

        public SaveService() { }

        public void SaveToSlot(string slotId, SaveData data) {
            if (data == null) return;

            data.Timestamp = DateTime.UtcNow.Ticks / 10000000;

            string activeScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            if (activeScene != "MainMenu") {
                data.SceneName = activeScene;
            }
            else if (string.IsNullOrEmpty(data.SceneName)) {
                data.SceneName = "Game";
            }

            string json = JsonUtility.ToJson(data);
            
            PlayerPrefs.SetString($"{SAVE_PREFIX}{slotId}", json);
            PlayerPrefs.Save();

            OnSaveCompleted?.Invoke(slotId);
        }

        public SaveData LoadFromSlot(string slotId) {
            string key = $"{SAVE_PREFIX}{slotId}";
            if (!PlayerPrefs.HasKey(key)) {
                return null;
            }

            string json = PlayerPrefs.GetString(key);
            SaveData data = JsonUtility.FromJson<SaveData>(json);
            return Normalize(data);
        }

        public void DeleteSlot(string slotId) {
            string key = $"{SAVE_PREFIX}{slotId}";
            
            if (PlayerPrefs.HasKey(key)) {
                PlayerPrefs.DeleteKey(key);
                PlayerPrefs.Save();
            }
        }

        public bool SlotExists(string slotId) {
            return PlayerPrefs.HasKey($"{SAVE_PREFIX}{slotId}");
        }

        public string[] ListSlots() {
            if (!SlotExists(AUTOSAVE_SLOT)) {
                return Array.Empty<string>();
            }

            return new string[] { AUTOSAVE_SLOT };
        }

        public void SaveCheckpoint(string checkpointId, SaveData data) {
            data.LastCheckpointId = checkpointId;
            SaveToSlot(checkpointId, data);
        }

        private static SaveData Normalize(SaveData data) {
            if (data == null) {
                return null;
            }

            if (string.IsNullOrEmpty(data.SceneName)) {
                data.SceneName = "Game";
            }

            data.MissionProgress ??= new Dictionary<string, string>();
            data.InventoryItemIds ??= new List<string>();
            data.PlayerPosition ??= new Vector3Data();
            data.PlayerRotation ??= new QuaternionData();
            data.CameraTargetPosition ??= new Vector3Data();
            data.CameraTargetRotation ??= new QuaternionData();
            return data;
        }
    }
}
