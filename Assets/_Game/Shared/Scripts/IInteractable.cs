// Autor: Murillo Gomes Yonamine
// Data: 08/03/2026

using UnityEngine;

namespace FifthSemester.Shared {
    public interface IInteractable {
        string Id { get; }

        bool IsInteractable { get; }
        public void Interact();
        public void StopInteract();

        void Highlight(bool value);
    }
}