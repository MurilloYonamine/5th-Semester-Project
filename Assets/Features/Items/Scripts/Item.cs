using UnityEngine;
using ThirdParty.QuickOutline;

namespace FifthSemester.Items {
    public class Item : MonoBehaviour, IInteractable {
        public bool IsInteractable => true;
        [SerializeField] private Outline _outline;

        private void Awake() {
            _outline = GetComponent<Outline>();
            _outline.enabled = true;
        }

        public void Interact() {
            Debug.Log("Pode Interagir com o Item!");
            _outline.enabled = false;
            Destroy(gameObject);
        }

        public void StopInteract() {
            Debug.Log("Não pode interagir com o Item!");
        }

        public override string ToString() {
            var itemName = gameObject.name ?? "Unnamed Item";
            return $"{itemName}";
        }
    }
}
