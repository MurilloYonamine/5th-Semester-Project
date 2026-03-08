// Autor: Murillo Gomes Yonamine
// Data: 08/03/2026

using UnityEngine;

namespace FifthSemester.Items {
    public interface IInteractable {
        [field: SerializeField] public bool IsInteractable { get; }
        public void Interact();
        public void StopInteract();
    }
}