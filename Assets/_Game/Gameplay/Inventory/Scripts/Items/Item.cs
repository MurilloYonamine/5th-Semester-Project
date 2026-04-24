// autor: Murillo Gomes Yonamine
// data: 08/03/2026

using UnityEngine;
using ThirdParty.QuickOutline;
using FifthSemester.Player;
using FifthSemester.Gameplay.Shared;

namespace FifthSemester.Gameplay.Inventory {
    [RequireComponent(typeof(Outline))]
    public class Item : MonoBehaviour, IInteractable {
        public bool IsInteractable => true;
        private Outline _outline;
        private BoxCollider _collider;

        private void Awake() {
            _outline = GetComponent<Outline>();
            _collider = GetComponent<BoxCollider>();

            _outline.enabled = false;
            _collider.enabled = true;
        }

        public void Interact() {
            Debug.Log("Interagiu com o item!");
            _outline.enabled = false;
            _collider.enabled = false;
        }

        public void StopInteract() {
            Debug.Log("Não pode interagir com o Item!");
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