using System.Collections.Generic;
using FifthSemester.Core.Events;
using FifthSemester.Core.Services;
using FifthSemester.Gameplay.Inventory;
using UnityEngine;
using UnityEngine.Playables;

namespace FifthSemester.Gameplay.Map2 {
    public class Map2KeyService : MonoBehaviour, IMap2KeyService {
        [Header("Timeline")]
        [SerializeField] private PlayableDirector _allKeysCollectedTimeline;

        private List<Map2KeyItem> _registeredKeys = new List<Map2KeyItem>();
        private IInventoryService<Item> _inventoryService;
        private IEventBus _eventBus;
        private bool _played;

        public bool HasCollectedAllKeys { get; private set; }

        private void Awake() {
            ServiceLocator.Register<IMap2KeyService>(this);
            _eventBus = ServiceLocator.Get<IEventBus>();
            _inventoryService = ServiceLocator.Get<IInventoryService<Item>>();
        }

        private void Start() {
            _eventBus?.Subscribe<ItemPickedUpEvent>(OnItemPickedUp);
        }

        private void OnDestroy() {
            _eventBus?.Unsubscribe<ItemPickedUpEvent>(OnItemPickedUp);
            ServiceLocator.TryGet<IMap2KeyService>(out var dummy);
        }

        public void RegisterKey(Map2KeyItem key) {
            if (key == null) return;
            if (!_registeredKeys.Contains(key)) _registeredKeys.Add(key);
        }

        public void UnregisterKey(Map2KeyItem key) {
            // Keep the original list count stable so the total key amount is always based on the list.
        }

        private void OnItemPickedUp(ItemPickedUpEvent evt) {
            if (_played) return;
            if (evt.ItemGameObject == null) return;

            Map2KeyItem picked = evt.ItemGameObject.GetComponent<Map2KeyItem>();
            if (picked == null) return;

            // Count how many registered keys are present in inventory
            if (_inventoryService == null) return;

            var items = _inventoryService.GetItems();
            if (items == null) return;

            int keyCount = 0;
            for (int i = 0; i < items.Count; i++) {
                if (items[i] is Map2KeyItem) keyCount++;
            }

            int total = _registeredKeys.Count;
            if (total <= 0) return;

            if (keyCount >= total) {
                KeepOnlyLatestKey(picked, items);
                _played = true;
                HasCollectedAllKeys = true;

                if (_allKeysCollectedTimeline != null) {
                    try { _allKeysCollectedTimeline.Play(); } catch { }
                }
            }
        }

        private void KeepOnlyLatestKey(Map2KeyItem latestPicked, IReadOnlyList<Item> items) {
            if (_inventoryService == null || latestPicked == null || items == null) {
                return;
            }

            List<Map2KeyItem> keysToRemove = new List<Map2KeyItem>();
            for (int i = 0; i < items.Count; i++) {
                if (items[i] is not Map2KeyItem keyItem) {
                    continue;
                }

                if (keyItem == latestPicked) {
                    continue;
                }

                keysToRemove.Add(keyItem);
            }

            for (int i = 0; i < keysToRemove.Count; i++) {
                Map2KeyItem keyToRemove = keysToRemove[i];
                _inventoryService.RemoveItem(keyToRemove);

                if (keyToRemove != null) {
                    keyToRemove.gameObject.SetActive(false);
                }
            }
        }

        public bool TryPrepareForLastKey(Map2KeyItem lastKey) {
            if (_played || _inventoryService == null) return false;

            var items = _inventoryService.GetItems();
            if (items == null) return false;

            int keyCount = 0;
            List<Map2KeyItem> keysInInventory = new List<Map2KeyItem>();
            for (int i = 0; i < items.Count; i++) {
                if (items[i] is Map2KeyItem k) {
                    keyCount++;
                    keysInInventory.Add(k);
                }
            }

            int total = _registeredKeys.Count;
            if (total <= 0) return false;

            if (keyCount + 1 >= total) {
                for (int i = 0; i < keysInInventory.Count; i++) {
                    _inventoryService.RemoveItem(keysInInventory[i]);
                    if (keysInInventory[i] != null) {
                        keysInInventory[i].gameObject.SetActive(false);
                    }
                }

                _played = true;
                HasCollectedAllKeys = true;

                if (_allKeysCollectedTimeline != null) {
                    try { _allKeysCollectedTimeline.Play(); } catch { }
                }
                
                return true;
            }

            return false;
        }
    }
}
