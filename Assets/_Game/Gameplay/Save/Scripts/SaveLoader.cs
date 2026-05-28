// Autor: Murillo Gomes Yonamine
// Data: 05/05/2026

using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine;
using FifthSemester.Core.Services;
using FifthSemester.Gameplay.Inventory;
using FifthSemester.Player;
using FifthSemester.Player.Components;
using FifthSemester.Gameplay.Missions;

namespace FifthSemester.Gameplay.Save {
    public static class SaveLoader {
        private static SaveData _pending;

        public static bool IsPendingSave => _pending != null;

        public static void SetPendingSave(SaveData data) {
            _pending = data;
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        public static void ClearPendingSave() {
            _pending = null;
            SceneManager.sceneLoaded -= OnSceneLoaded;
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
            PlayerController player = Object.FindFirstObjectByType<PlayerController>();
            IMissionService missionService = ServiceLocator.Get<IMissionService>();
            IInventoryService<Item> inventoryService = ServiceLocator.Get<IInventoryService<Item>>();

            if (player != null) {
                player.transform.position = _pending.PlayerPosition.ToVector3();
                player.transform.rotation = _pending.PlayerRotation.ToQuaternion();

                PlayerCamera playerCamera = player.PlayerCamera;
                if (playerCamera != null) {
                    Transform cameraTarget = playerCamera.GetCameraTarget();
                    if (cameraTarget != null) {
                        cameraTarget.position = _pending.CameraTargetPosition.ToVector3();
                        cameraTarget.rotation = _pending.CameraTargetRotation.ToQuaternion();
                    }
                }
            } 

            if (missionService != null) {
                missionService.SkipToMission(_pending.CurrentMissionIndex);
            }

            if (inventoryService != null && _pending.InventoryItemIds.Count > 0) {
                LoadInventoryItems(inventoryService, _pending.InventoryItemIds);
            }

            _pending = null;
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private static void LoadInventoryItems(IInventoryService<Item> inventoryService, System.Collections.Generic.IReadOnlyList<string> itemIds) {
            IItemRegistry<Item> itemRegistry = ServiceLocator.Get<IItemRegistry<Item>>();
            if (itemRegistry == null) {
                return;
            }

            for (int i = 0; i < itemIds.Count; i++) {
                string itemId = itemIds[i];
                if (string.IsNullOrWhiteSpace(itemId)) {
                    continue;
                }

                Item item = itemRegistry.InstantiateItem(itemId);
                if (item != null) {
                    inventoryService.AddItem(item);
                }
            }
        }
    }
}
