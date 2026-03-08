using System;
using System.Collections.Generic;
using UnityEngine;
using FifthSemester.Items;

namespace FifthSemester.Inventory {
    public class InventoryController {
        private List<IInteractable> _items;
        private bool _isInventoryOpen = false;

        public bool IsInventoryOpen => _isInventoryOpen;
        public event Action<bool> OnInventoryToggled;
        public event Action<IInteractable, int> OnItemAdded;
        public event Action<int> OnItemRemoved;

        public InventoryController() {
            _items = new List<IInteractable>();
        }

        public void AddItem(IInteractable item) {
            if (_items.Contains(item)) {
                Debug.LogWarning($"Attempted to add {item}, but it is already in inventory.");
                return;
            }

            _items.Add(item);
            int index = _items.Count - 1;

            OnItemAdded?.Invoke(item, index);
            Debug.Log($"Added {item.ToString()} to inventory at slot {index}.");
        }

        public void RemoveItem(IInteractable item) {
            int index = _items.IndexOf(item);

            if (index < 0) {
                Debug.LogWarning($"Attempted to remove {item}, but it is not in inventory.");
                return;
            }

            _items.RemoveAt(index);
            OnItemRemoved?.Invoke(index);
            Debug.Log($"Removed {item.ToString()} from inventory.");
        }

        public void ToggleInventory() {
            _isInventoryOpen = !_isInventoryOpen;
            OnInventoryToggled?.Invoke(_isInventoryOpen);
        }

        public void CloseInventory() {
            if (_isInventoryOpen) {
                _isInventoryOpen = false;
                OnInventoryToggled?.Invoke(false);
            }
        }
    }
}
