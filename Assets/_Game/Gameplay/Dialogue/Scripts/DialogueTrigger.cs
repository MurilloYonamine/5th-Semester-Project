// autor: Murillo Gomes Yonamine
// data: 30/03/2026

using UnityEngine;
using ThirdParty.QuickOutline;
using FifthSemester.Core.Services;

namespace FifthSemester.Gameplay.Dialogue {
    [RequireComponent(typeof(Outline))]
    public class DialogueTrigger : MonoBehaviour {
        [SerializeField] private DialogueSO _dialogue;
        private Outline _outline;

        private IDialogueService<DialogueSO> _dialogueService;

        private void Awake() {
            _outline = GetComponent<Outline>();
        }
        private void Start() {
            _dialogueService = ServiceLocator.Get<IDialogueService<DialogueSO>>();
        }

        public void TriggerDialogue() {
            if (_dialogue != null) {
                _dialogueService.StartDialogue(_dialogue);
            }
        }

        public void TurnOutline(bool enable) {
            if (_outline != null) {
                _outline.enabled = enable;
            }
        }

        public void SetDialogue(DialogueSO newDialogue) {
            _dialogue = newDialogue;
        }

        public bool goBackToMenuOnEnd = false;
        public void GoBackToMenu() {

        }
    }
}
