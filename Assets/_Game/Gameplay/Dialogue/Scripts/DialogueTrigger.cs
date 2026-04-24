// autor: Murillo Gomes Yonamine
// data: 30/03/2026

using UnityEngine;
using ThirdParty.QuickOutline;
using FifthSemester.Core.Services;
using FifthSemester.Gameplay.Shared;

namespace FifthSemester.Gameplay.Dialogue {
    [RequireComponent(typeof(Outline))]
    public class DialogueTrigger : MonoBehaviour, IInteractable {
        [SerializeField] private DialogueSO _dialogue;
        private Outline _outline;

        private IDialogueService<DialogueSO> _dialogueService;

        public bool IsInteractable => _dialogue != null;

        private void Awake() {
            _outline = GetComponent<Outline>();
            _outline.enabled = false;
        }
        private void Start() {
            _dialogueService = ServiceLocator.Get<IDialogueService<DialogueSO>>();
        }
        public void Interact() {
            if (_dialogue != null) {
                _dialogueService.StartDialogue(_dialogue);
            }
        }

        public void StopInteract() {
            _dialogueService.EndDialogue();
        }

        public void Highlight(bool value) {
            _outline.enabled = value;
        }
    }
}
