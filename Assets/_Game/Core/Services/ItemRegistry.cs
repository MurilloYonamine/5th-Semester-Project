// Autor: Murillo Gomes Yonamine
// Data: 05/05/2026

using System.Collections.Generic;
using UnityEngine;

namespace FifthSemester.Core.Services {
    public class ItemRegistry<TItem> : IItemRegistry<TItem> where TItem : MonoBehaviour {
        private Dictionary<string, TItem> _itemPrefabs = new();

        public void RegisterItemPrefab(string itemId, TItem prefab) {
            if (string.IsNullOrEmpty(itemId) || prefab == null) {
                Debug.LogWarning("[ItemRegistry] Cannot register null prefab or empty itemId.");
                return;
            }

            if (!_itemPrefabs.ContainsKey(itemId)) {
                _itemPrefabs[itemId] = prefab;
                Debug.Log($"[ItemRegistry] Registered item: {itemId}");
            } else {
                Debug.LogWarning($"[ItemRegistry] Item already registered: {itemId}");
            }
        }

        public TItem GetItemPrefab(string itemId) {
            _itemPrefabs.TryGetValue(itemId, out var prefab);
            return prefab;
        }

        public TItem InstantiateItem(string itemId) {
            var prefab = GetItemPrefab(itemId);
            if (prefab == null) {
                Debug.LogWarning($"[ItemRegistry] Item prefab not found: {itemId}");
                return null;
            }

            TItem instance = Object.Instantiate(prefab);
            return instance;
        }
    }
}
