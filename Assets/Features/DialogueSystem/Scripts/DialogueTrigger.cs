// autor: Murillo Gomes Yonamine
// data: 30/03/2026

using FifthSemester.Core.Managers;
using ThirdParty.QuickOutline;
using UnityEngine;

namespace FifthSemester.DialogueSystem {
    [RequireComponent(typeof(Outline))]
    public class DialogueTrigger : MonoBehaviour {
        [SerializeField] private DialogueSO _dialogue;
        private Outline _outline;

        private void Awake() {
            _outline = GetComponent<Outline>();
        }

        public void TriggerDialogue() {
            if (_dialogue != null && DialogueManager.Instance != null) {
                DialogueManager.Instance.StartDialogue(_dialogue);
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
            if (goBackToMenuOnEnd) {
                SceneLoaderManager.Instance.LoadNewLevel("MainMenu");
            }
        }
    }
}
