// autor: Murillo Gomes Yonamine
// data: 30/03/2026

using FifthSemester.Core.Services;
using FifthSemester.Gameplay.Shared;
using Sirenix.OdinInspector;
using ThirdParty.QuickOutline;
using UnityEngine;
using UnityEngine.Playables;

namespace FifthSemester.Gameplay.Dialogue {
    [RequireComponent(typeof(Outline))]
    public class DialogueTrigger : MonoBehaviour, IInteractable {
        [field: SerializeField] public string Id { get; private set; }

        [SerializeField] private TextAsset _dialogue;
        private Outline _outline;

        private IDialogueService<TextAsset> _dialogueService;

        public bool IsInteractable => _dialogue != null;
        [SerializeField] private PlayableDirector _director;

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

            _dialogueService.StartDialogue(_dialogue, _director);
        }

        public void StopInteract() {
            _dialogueService.EndDialogue();
        }

        public void Highlight(bool value) {
            _outline.enabled = value;
        }
    }
}
