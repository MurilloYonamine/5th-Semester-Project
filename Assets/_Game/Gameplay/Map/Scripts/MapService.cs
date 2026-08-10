// Autor: Murillo Gomes Yonamine
// Data: 19/05/2026

using FifthSemester.Core.Enums;
using FifthSemester.Core.Services;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

namespace FifthSemester.Gameplay.Map {
    public class MapService : SerializedMonoBehaviour, IMapService {
        [SerializeField]
<<<<<<< HEAD
        private Dictionary<string, GameObject> _registry;
=======
        private readonly Dictionary<string, GameObject> _registry = new();
>>>>>>> origin/main

        private void Awake() {
            ServiceLocator.Register<IMapService>(this);
        }

        public void Register(string id, GameObject obj) {
            if (string.IsNullOrEmpty(id) || obj == null) return;
            if (_registry.ContainsKey(id)) return;
            _registry.Add(id, obj);
        }

        public void Register(DoorType doorType, GameObject obj) {
            if (doorType == DoorType.None) return;
            Register(doorType.ToString(), obj);
        }

        public void Unregister(string id) {
            if (string.IsNullOrEmpty(id)) return;
            if (!_registry.ContainsKey(id)) return;
            _registry.Remove(id);
        }

        public void Unregister(DoorType doorType) {
            if (doorType == DoorType.None) return;
            Unregister(doorType.ToString());
        }

        public GameObject Get(string id) {
            if (string.IsNullOrEmpty(id)) return null;
            _registry.TryGetValue(id, out GameObject obj);
            return obj;
        }

        public GameObject Get(DoorType doorType) {
            if (doorType == DoorType.None) return null;
            return Get(doorType.ToString());
        }
    }
}
