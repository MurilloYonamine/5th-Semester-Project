using System.Linq;
using FifthSemester.Core.Events;
using FifthSemester.Core.Services;
using FifthSemester.Gameplay.Inventory;
using UnityEngine;
using UnityEngine.Playables;

namespace FifthSemester.Gameplay.Map2 {
    [DisallowMultipleComponent]
    public class Map2KeyManager : MonoBehaviour {
        [Header("Timeline")]
        [SerializeField] private PlayableDirector _allKeysCollectedTimeline;

        [Header("Options")]
        [Tooltip("Override auto-detected total keys. Use <= 0 to auto-detect.")]
        [SerializeField] private int _requiredKeys = -1;

        private IInventoryService<Item> _inventoryService;
        private IEventBus _eventBus;
        private int _totalKeys;
        private bool _played;

        private void Awake() {
            _eventBus = ServiceLocator.Get<IEventBus>();
            _inventoryService = ServiceLocator.Get<IInventoryService<Item>>();
        }

        private void Start() {
            if (_requiredKeys > 0) {
                _totalKeys = _requiredKeys;
            }
            else {
                var keys = FindObjectsOfType<Map2KeyItem>(true);
                _totalKeys = keys != null ? keys.Length : 0;
            }

            _eventBus?.Subscribe<ItemPickedUpEvent>(OnItemPickedUp);
        }

        private void OnDestroy() {
            _eventBus?.Unsubscribe<ItemPickedUpEvent>(OnItemPickedUp);
        }

        private void OnItemPickedUp(ItemPickedUpEvent evt) {
            if (_played) return;

            if (evt.ItemGameObject == null) return;

            if (evt.ItemGameObject.GetComponent<Map2KeyItem>() == null) return;

            if (_inventoryService == null) return;

            var items = _inventoryService.GetItems();
            if (items == null) return;

            int keyCount = 0;
            for (int i = 0; i < items.Count; i++) {
                if (items[i] is Map2KeyItem) keyCount++;
            }

            if (keyCount >= _totalKeys && _allKeysCollectedTimeline != null) {
                _played = true;
                try {
                    _allKeysCollectedTimeline.Play();
                }
                catch {
                    // swallow to avoid breaking gameplay if director errors
                }
            }
        }
    }
}
