using FifthSemester.Dialogue;
using FifthSemester.Framework;
using ThirdParty.QuickOutline;
using UnityEngine;

namespace FifthSemester.Characters {
    [RequireComponent(typeof(Outline))]
    [RequireComponent(typeof(BoxCollider))]
    public class Character : MonoBehaviour, IInteractable {
        public bool IsInteractable { get; private set; }

        public Collider Collider { get; private set; }
        public Outline Outline { get; private set; }

        private void Awake() {
            Collider = GetComponent<Collider>();
            Outline = GetComponent<Outline>();
            IsInteractable = true;
        }
        private void OnEnable() {
            DialogueSystem.Instance.OnDialogueStarted += HandleDialogueStarted;
        }

        private void OnDisable() {
            DialogueSystem.Instance.OnDialogueStarted -= HandleDialogueStarted;
        }

        private void HandleDialogueStarted(DialogueContainer dialogueContainer) {
        }

        public void Interact() {
            DialogueSystem.Instance.StartDialogue();
        }
        public void StopInteract() {
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
            var itemName = gameObject.name ?? "Unnamed Character";
            return $"{itemName}";
        }
    }
}
