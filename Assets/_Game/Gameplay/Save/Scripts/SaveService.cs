// Autor: Murillo Gomes Yonamine
// Data: 05/05/2026

using UnityEngine;
using System;
using System.Collections.Generic;
using FifthSemester.Core.Services;

namespace FifthSemester.Gameplay.Save{
    public class SaveService : ISaveService {
        private const string SAVE_PREFIX = "save_";
<<<<<<< HEAD
        private const string AUTOSAVE_SLOT = "default";

        public event Action<string> OnSaveCompleted;

        public SaveService() { }
=======
        private const string DEFAULT_SLOT = "default";

        public event Action<string> OnSaveCompleted;

        public SaveService() {
            if (!SlotExists(DEFAULT_SLOT)) {
                SaveToSlot(DEFAULT_SLOT, new SaveData());
            }
        }
>>>>>>> origin/main

        public void SaveToSlot(string slotId, SaveData data) {
            if (data == null) return;

            data.Timestamp = DateTime.UtcNow.Ticks / 10000000;
<<<<<<< HEAD

            string activeScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            if (activeScene != "MainMenu") {
                data.SceneName = activeScene;
            }
            else if (string.IsNullOrEmpty(data.SceneName)) {
                data.SceneName = "Game";
            }

=======
>>>>>>> origin/main
            string json = JsonUtility.ToJson(data);
            
            PlayerPrefs.SetString($"{SAVE_PREFIX}{slotId}", json);
            PlayerPrefs.Save();

            OnSaveCompleted?.Invoke(slotId);
<<<<<<< HEAD
=======
            Debug.Log($"[SaveService] Saved to slot: {slotId}");
>>>>>>> origin/main
        }

        public SaveData LoadFromSlot(string slotId) {
            string key = $"{SAVE_PREFIX}{slotId}";
            if (!PlayerPrefs.HasKey(key)) {
<<<<<<< HEAD
=======
                Debug.LogWarning($"[SaveService] Slot not found: {slotId}");
>>>>>>> origin/main
                return null;
            }

            string json = PlayerPrefs.GetString(key);
            SaveData data = JsonUtility.FromJson<SaveData>(json);
<<<<<<< HEAD
            return Normalize(data);
=======
            Debug.Log($"[SaveService] Loaded from slot: {slotId}");
            return data;
>>>>>>> origin/main
        }

        public void DeleteSlot(string slotId) {
            string key = $"{SAVE_PREFIX}{slotId}";
            
            if (PlayerPrefs.HasKey(key)) {
                PlayerPrefs.DeleteKey(key);
                PlayerPrefs.Save();
<<<<<<< HEAD
=======
                Debug.Log($"[SaveService] Deleted slot: {slotId}");
>>>>>>> origin/main
            }
        }

        public bool SlotExists(string slotId) {
            return PlayerPrefs.HasKey($"{SAVE_PREFIX}{slotId}");
        }

        public string[] ListSlots() {
<<<<<<< HEAD
            if (!SlotExists(AUTOSAVE_SLOT)) {
                return Array.Empty<string>();
            }

            return new string[] { AUTOSAVE_SLOT };
=======
            List<string> slots = new();
            int i = 0;
            while (PlayerPrefs.HasKey($"{SAVE_PREFIX}slot_{i}")) {
                slots.Add($"slot_{i}");
                i++;
            }
            if (SlotExists(DEFAULT_SLOT)) slots.Insert(0, DEFAULT_SLOT);
            return slots.ToArray();
>>>>>>> origin/main
        }

        public void SaveCheckpoint(string checkpointId, SaveData data) {
            data.LastCheckpointId = checkpointId;
            SaveToSlot(checkpointId, data);
        }
<<<<<<< HEAD

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
=======
>>>>>>> origin/main
    }
}
