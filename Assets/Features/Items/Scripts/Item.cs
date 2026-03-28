// autor: Murillo Gomes Yonamine
// data: 08/03/2026

using UnityEngine;
using ThirdParty.QuickOutline;
using FifthSemester.Framework;

namespace FifthSemester.Items {
    [RequireComponent(typeof(Outline))]
    [RequireComponent(typeof(BoxCollider))]
    public class Item : MonoBehaviour, IInteractable {
        public bool IsInteractable => true;
        public Outline Outline { get; private set; }
        public Collider Collider { get; private set; }

        private void Awake() {
            Outline = GetComponent<Outline>();
            Collider = GetComponent<BoxCollider>();

            Outline.enabled = false;
            Collider.enabled = true;
        }

        public void Interact() {
            Debug.Log("Interagiu com o item!");
            Outline.enabled = false;
            Collider.enabled = false;
        }

        public void StopInteract() {
            Debug.Log("Não pode interagir com o Item!");
        }

        public void Highlight() {
            if (Outline != null) {
                Outline.enabled = true;
            }
        }
        public void Unhighlight() {
            if (Outline != null) {
                Outline.enabled = false;
            }
        }
        public override string ToString() {
            var itemName = gameObject.name ?? "Unnamed Item";
            return $"{itemName}";
        }
    }
}
