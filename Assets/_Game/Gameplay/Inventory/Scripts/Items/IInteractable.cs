// Autor: Murillo Gomes Yonamine
// Data: 08/03/2026

using FifthSemester.Player;
using UnityEngine;

namespace FifthSemester.Gameplay.Inventory {
    public interface IInteractable {
        bool IsInteractable { get; }
        public void Interact(PlayerInteraction player);
        public void StopInteract();

        void Highlight();
        void Unhighlight();
    }
}