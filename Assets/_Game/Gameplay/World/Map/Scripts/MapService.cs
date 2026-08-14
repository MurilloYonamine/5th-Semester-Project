// Autor: Murillo Gomes Yonamine
// Data: 19/05/2026

using FifthSemester.Core.Enums;
using FifthSemester.Core.Services;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

namespace FifthSemester.Gameplay {
    public class MapService : SerializedMonoBehaviour, IMapService {
        private const string TAG = "<color=yellow><b>[MapService]</b></color>";

        [SerializeField]
        private Dictionary<string, GameObject> _registry;

        private void Awake() {
            ServiceLocator.Register<IMapService>(this);
        }

        private void OnDestroy() {
            ServiceLocator.Unregister<IMapService>();
        }

        public void Register(string id, GameObject obj) {
            if (string.IsNullOrEmpty(id) || obj == null) return;
            if (_registry == null) _registry = new Dictionary<string, GameObject>();
            if (_registry.ContainsKey(id)) {
                _registry[id] = obj;
                Debug.Log($"{TAG} Updated registration for '{id}' -> GameObject '{obj.name}'");
                return;
            }
            _registry.Add(id, obj);
            Debug.Log($"{TAG} Registered '{id}' -> GameObject '{obj.name}' (Total registered: {_registry.Count})");
        }

        public void Register(DoorType doorType, GameObject obj) {
            if (doorType == DoorType.None) return;
            Register(doorType.ToString(), obj);
        }

        public void Unregister(string id) {
            if (string.IsNullOrEmpty(id) || _registry == null) return;
            if (!_registry.ContainsKey(id)) return;
            _registry.Remove(id);
            Debug.Log($"{TAG} Unregistered '{id}'");
        }

        public void Unregister(DoorType doorType) {
            if (doorType == DoorType.None) return;
            Unregister(doorType.ToString());
        }

        public GameObject Get(string id) {
            if (string.IsNullOrEmpty(id) || _registry == null) return null;
            _registry.TryGetValue(id, out GameObject obj);
            return obj;
        }

        public GameObject Get(DoorType doorType) {
            if (doorType == DoorType.None) return null;
            return Get(doorType.ToString());
        }
    }
}
