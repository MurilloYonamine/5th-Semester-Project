using System.Collections.Generic;
using FifthSemester.Core.Events;
using FifthSemester.Core.Services;
using FifthSemester.Gameplay.Inventory;
using UnityEngine;
using UnityEngine.Playables;

namespace FifthSemester.Gameplay.Map2 {
    [DisallowMultipleComponent]
    public class Map2KeyService : MonoBehaviour, IMap2KeyService {
        [Header("Timeline")]
        [SerializeField] private PlayableDirector _allKeysCollectedTimeline;

        [Header("Options")]
        [Tooltip("Override auto-detected total keys. Use <= 0 to auto-detect.")]
        [SerializeField] private int _requiredKeys = -1;

        private List<Map2KeyItem> _registeredKeys = new List<Map2KeyItem>();
        private IInventoryService<Item> _inventoryService;
        private IEventBus _eventBus;
        private bool _played;

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
            if (key == null) return;
            _registeredKeys.Remove(key);
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

            int total = _requiredKeys > 0 ? _requiredKeys : _registeredKeys.Count;
            if (total <= 0) return;

            if (keyCount >= total && _allKeysCollectedTimeline != null) {
                _played = true;
                try { _allKeysCollectedTimeline.Play(); } catch { }
            }
        }
    }
}
