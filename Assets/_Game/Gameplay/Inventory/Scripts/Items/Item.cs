// autor: Murillo Gomes Yonamine
// data: 08/03/2026

using UnityEngine;
using ThirdParty.QuickOutline;
using FifthSemester.Player;
using FifthSemester.Gameplay.Shared;

namespace FifthSemester.Gameplay.Inventory {
    [RequireComponent(typeof(Outline))]
    public class Item : MonoBehaviour, IInteractable {
        [field: SerializeField] public string Id { get; private set; }

        public bool IsInteractable => true;
        private Outline _outline;
        private BoxCollider _collider;

        private void Awake() {
            _outline = GetComponent<Outline>();

            if (!TryGetComponent(out BoxCollider collider)) {
                _collider = GetComponentInChildren<BoxCollider>();
            } else {
                _collider = collider;
            }

            _outline.enabled = false;

            if (_collider != null)
                _collider.enabled = true;
        }

        public void Interact() {
            _outline.enabled = false;
            _collider.enabled = false;
        }

        public void StopInteract() {
        }

        public void Highlight(bool value) {
            _outline.enabled = value;
        }
        public override string ToString() {
            var itemName = gameObject.name ?? "Unnamed Item";
            return $"{itemName}";
        }
    }
}
