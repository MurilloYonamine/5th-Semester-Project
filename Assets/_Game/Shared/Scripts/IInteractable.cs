// Autor: Murillo Gomes Yonamine
// Data: 08/03/2026

using UnityEngine;

namespace FifthSemester.Gameplay.Shared {
    public interface IInteractable {
        bool IsInteractable { get; }
        public void Interact();
        public void StopInteract();

        void Highlight(bool value);
    }
}