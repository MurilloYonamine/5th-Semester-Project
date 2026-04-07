// autor: Murillo Gomes Yonamine
// data: 08/03/2026

using UnityEngine;
using ThirdParty.QuickOutline;

namespace FifthSemester.Items {
    public class Item : MonoBehaviour, IInteractable {
        public bool IsInteractable => true;
        [SerializeField] private Outline _outline;
        [SerializeField] private BoxCollider _collider;

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

        public void Highlight() {
            if (_outline != null) {
                _outline.enabled = true;
            }
        }
        public void Unhighlight() {
            if (_outline != null) {
                _outline.enabled = false;
            }
        }
        public override string ToString() {
            var itemName = gameObject.name ?? "Unnamed Item";
            return $"{itemName}";
        }
    }
}
