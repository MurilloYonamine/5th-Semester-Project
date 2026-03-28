// Autor: Murillo Gomes Yonamine
// Data: 08/03/2026

using UnityEngine;
using ThirdParty.QuickOutline;

namespace FifthSemester.Framework {
    public interface IInteractable {
        [SerializeField] public bool IsInteractable { get; }
        public Collider Collider { get; }
        public Outline Outline { get; }

        public void Interact();
        public void StopInteract();

        public void Highlight();
        public void Unhighlight();
    }
}
