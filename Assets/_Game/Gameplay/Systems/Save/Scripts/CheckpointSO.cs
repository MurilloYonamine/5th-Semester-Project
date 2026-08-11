// Autor: Murillo Gomes Yonamine
// Data: 05/05/2026

using UnityEngine;

namespace FifthSemester.Gameplay {
    [CreateAssetMenu(fileName = "Checkpoint", menuName = "Game/Checkpoint", order = 1)]
    public class CheckpointSO : ScriptableObject {
        [SerializeField] private string _id;
        [SerializeField] private string _displayName;
        [SerializeField] private string _description;
        [SerializeField] private Sprite _icon;
        [SerializeField] private Vector3 _defaultSpawnPosition;

        public string Id => _id;
        public string DisplayName => _displayName;
        public string Description => _description;
        public Sprite Icon => _icon;
        public Vector3 DefaultSpawnPosition => _defaultSpawnPosition;

        private void OnValidate() {
            if (string.IsNullOrWhiteSpace(_id)) {
                _id = name;
            }
        }
    }
}
