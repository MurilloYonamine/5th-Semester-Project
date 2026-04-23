using System;
using System.Collections.Generic;
using UnityEngine;
using FifthSemester.Core.Services;
using FifthSemester.Core.Events;

namespace FifthSemester.Gameplay.Inventory {
    public class InventoryService : IInventoryService<Item>, IDisposable {
        private int _maxCapacity;
        private List<Item> _items = new List<Item>();

        public event Action<Item> OnItemAdded;
        public event Action<Item> OnItemRemoved;

        private IEventBus _eventBus;

        public InventoryService(int maxCapacity = 6) {
            _maxCapacity = maxCapacity;
            _eventBus = ServiceLocator.Get<IEventBus>();
        }

        public void Dispose() {
            _items.Clear();
            _eventBus = null;
        }

        public bool AddItem(Item item) {
            if (item == null || _items.Count >= _maxCapacity) {
                Debug.LogWarning("Inventário cheio ou item nulo.");
                return false;
            }

            _items.Add(item);

            if (item is MonoBehaviour mono) {
                mono.gameObject.SetActive(false);
            }

            OnItemAdded?.Invoke(item);
            _eventBus?.Publish(new InventoryItemAddedEvent(_items.Count - 1, _items.Count));

            return true;
        }

        public bool RemoveItem(Item item) {
            int index = _items.IndexOf(item);

            if (item != null && _items.Remove(item)) {

                if (item is MonoBehaviour mono) {
                    mono.gameObject.SetActive(true);
                }

                OnItemRemoved?.Invoke(item);
                _eventBus?.Publish(new InventoryItemRemovedEvent(index, _items.Count));

                return true;
            }

            return false;
        }

        public bool HasItem(Item item) => _items.Contains(item);
        public IReadOnlyList<Item> GetItems() => _items.AsReadOnly();
    }
}