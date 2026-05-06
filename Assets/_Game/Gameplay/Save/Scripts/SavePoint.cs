// Autor: Murillo Gomes Yonamine
// Data: 05/05/2026

using UnityEngine;
using System.Collections.Generic;
using FifthSemester.Core.Services;
using FifthSemester.Core.Events;
using FifthSemester.Gameplay.Missions;
using FifthSemester.Gameplay.Shared;
using FifthSemester.Gameplay.Inventory;
using FifthSemester.Player;
using FifthSemester.Player.Components;
using ThirdParty.QuickOutline;

namespace FifthSemester.Gameplay.Save {
    public class SavePoint : MonoBehaviour, IInteractable {
        [SerializeField] private CheckpointSO _checkpoint;
        [SerializeField] private Outline _outline;

        private ISaveService _saveService;
        private IMissionService _missionService;
        private IEventBus _eventBus;
        private IInventoryService<Item> _inventoryService;
        private PlayerController _playerController;

        public string Id => _checkpoint != null ? _checkpoint.Id : "unknown";
        public bool IsInteractable { get; private set; } = true;

        private void Start() {
            _saveService = ServiceLocator.Get<ISaveService>();
            _missionService = ServiceLocator.Get<IMissionService>() as MissionService;
            _eventBus = ServiceLocator.Get<IEventBus>();
            _inventoryService = ServiceLocator.Get<IInventoryService<Item>>();

            if (_outline == null) {
                _outline = GetComponent<Outline>();
            }
            Highlight(false);

            _eventBus?.Subscribe<SaveConfirmedEvent>(OnSaveConfirmed);
        }

        private void OnDisable() {
            _eventBus?.Unsubscribe<SaveConfirmedEvent>(OnSaveConfirmed);
        }

        public void Interact() {
            if (_checkpoint == null) return;
            _eventBus?.Publish(new SaveConfirmationRequestedEvent(_checkpoint.Id));
        }

        public void SetPlayerController(PlayerController playerController) {
            _playerController = playerController;
        }

        public void StopInteract() {
            _outline.enabled = false;
        }

        public void Highlight(bool value) {
            if (_outline == null) return;

            _outline.enabled = value;
        }

        private void OnSaveConfirmed(SaveConfirmedEvent evt) {
            if (_checkpoint == null || evt.CheckpointId != _checkpoint.Id) return;

            SaveGame();
        }

        private void SaveGame() {
            if (_saveService == null || _missionService == null || _playerController == null || _checkpoint == null) return;

            SaveData saveData = new() {
                CurrentMissionIndex = _missionService.CurrentIndex,
                PlayerPosition = new(_playerController.transform.position),
                PlayerRotation = new(_playerController.transform.rotation),
                LastCheckpointId = _checkpoint.Id
            };

            // Save camera target position and rotation
            PlayerCamera playerCamera = _playerController.PlayerCamera;
            if (playerCamera != null) {
                Transform cameraTarget = playerCamera.GetCameraTarget();
                if (cameraTarget != null) {
                    saveData.CameraTargetPosition = new(cameraTarget.position);
                    saveData.CameraTargetRotation = new(cameraTarget.rotation);
                }
            }

            // Save inventory items
            if (_inventoryService != null) {
                var items = _inventoryService.GetItems();
                foreach (var item in items) {
                    if (item is MonoBehaviour mono) {
                        saveData.InventoryItemIds.Add(mono.gameObject.name);
                    }
                }
            }

            _saveService.SaveCheckpoint(_checkpoint.Id, saveData);
            Debug.Log($"[SavePoint] Game saved at checkpoint: {_checkpoint.DisplayName}");
        }

        public void LoadGame(SaveData saveData) {
            if (saveData == null || _playerController == null) return;

            // Load player position and rotation
            _playerController.transform.position = saveData.PlayerPosition.ToVector3();
            _playerController.transform.rotation = saveData.PlayerRotation.ToQuaternion();

            // Load camera target if exists
            PlayerCamera playerCamera = _playerController.PlayerCamera;
            if (playerCamera != null) {
                Transform cameraTarget = playerCamera.GetCameraTarget();
                if (cameraTarget != null) {
                    cameraTarget.position = saveData.CameraTargetPosition.ToVector3();
                    cameraTarget.rotation = saveData.CameraTargetRotation.ToQuaternion();
                }
            }

            // Load mission progress
            if (_missionService != null) {
                _missionService.SkipToMission(saveData.CurrentMissionIndex);
            }

            // Load inventory items
            if (_inventoryService != null && saveData.InventoryItemIds.Count > 0) {
                LoadInventoryItems(saveData.InventoryItemIds);
            }

            Debug.Log($"[SavePoint] Game loaded from checkpoint: {_checkpoint.DisplayName}");
        }

        private void LoadInventoryItems(List<string> itemIds) {
            IItemRegistry<Item> itemRegistry = ServiceLocator.Get<IItemRegistry<Item>>();
            if (itemRegistry == null) {
                Debug.LogWarning("[SavePoint] IItemRegistry<Item> not found. Cannot load inventory.");
                return;
            }

            foreach (string itemId in itemIds) {
                Item item = itemRegistry.InstantiateItem(itemId);
                if (item != null) {
                    _inventoryService?.AddItem(item);
                } else {
                    Debug.LogWarning($"[SavePoint] Failed to instantiate item: {itemId}");
                }
            }
        }
    }
}
