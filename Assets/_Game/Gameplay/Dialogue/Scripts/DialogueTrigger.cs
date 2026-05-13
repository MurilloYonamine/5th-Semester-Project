// autor: Murillo Gomes Yonamine
// data: 30/03/2026

using UnityEngine;
using ThirdParty.QuickOutline;
using FifthSemester.Core.Services;
using FifthSemester.Gameplay.Shared;
using Sirenix.OdinInspector;

namespace FifthSemester.Gameplay.Dialogue {
    [RequireComponent(typeof(Outline))]
    public class DialogueTrigger : MonoBehaviour, IInteractable {
        [field: SerializeField] public string Id { get; private set; }

        [SerializeField] private TextAsset _dialogue;
        private Outline _outline;

        private IDialogueService<TextAsset> _dialogueService;

        public bool IsInteractable => _dialogue != null;

        private void Awake() {
            _outline = GetComponent<Outline>();
            _outline.enabled = false;
        }

        private void Start() {
            _dialogueService = ServiceLocator.Get<IDialogueService<TextAsset>>();
        }

        public void Interact() {
            if (_dialogue == null) {
                return;
            }

            _dialogueService.StartDialogue(_dialogue);
        }

        public void StopInteract() {
            _dialogueService.EndDialogue();
        }

        public void Highlight(bool value) {
            _outline.enabled = value;
        }
    }
}
