// Autor: Murillo Gomes Yonamine
// Data: 05/05/2026

using UnityEngine;
using System;
using System.Collections.Generic;
using FifthSemester.Core.Services;

namespace FifthSemester.Gameplay.Save{
    public class SaveService : ISaveService {
        private const string SAVE_PREFIX = "save_";
        private const string DEFAULT_SLOT = "default";

        public event Action<string> OnSaveCompleted;

        public SaveService() {
            if (!SlotExists(DEFAULT_SLOT)) {
                SaveToSlot(DEFAULT_SLOT, new SaveData());
            }
        }

        public void SaveToSlot(string slotId, SaveData data) {
            if (data == null) return;

            data.Timestamp = DateTime.UtcNow.Ticks / 10000000;
            string json = JsonUtility.ToJson(data);
            
            PlayerPrefs.SetString($"{SAVE_PREFIX}{slotId}", json);
            PlayerPrefs.Save();

            OnSaveCompleted?.Invoke(slotId);
        }

        public SaveData LoadFromSlot(string slotId) {
            string key = $"{SAVE_PREFIX}{slotId}";
            if (!PlayerPrefs.HasKey(key)) {
                Debug.LogWarning($"[SaveService] Slot not found: {slotId}");
                return null;
            }

            string json = PlayerPrefs.GetString(key);
            SaveData data = JsonUtility.FromJson<SaveData>(json);
            return data;
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
            List<string> slots = new();
            int i = 0;
            while (PlayerPrefs.HasKey($"{SAVE_PREFIX}slot_{i}")) {
                slots.Add($"slot_{i}");
                i++;
            }
            if (SlotExists(DEFAULT_SLOT)) slots.Insert(0, DEFAULT_SLOT);
            return slots.ToArray();
        }

        public void SaveCheckpoint(string checkpointId, SaveData data) {
            data.LastCheckpointId = checkpointId;
            SaveToSlot(checkpointId, data);
        }
    }
}
