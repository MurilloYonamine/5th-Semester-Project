using UnityEngine;

namespace FifthSemester.Dialogue {
    public class DialogueSystem : MonoBehaviour {
        public static DialogueSystem Instance { get; private set; }

        [SerializeField] private DialogueContainer _dialogueContainer;

        public delegate void DialogueStartedEventHandler();
        public event DialogueStartedEventHandler OnDialogueStarted;

        private void Awake() {
            if (Instance == null) {
                Instance = this;
            } else {
                Destroy(gameObject);
            }
        }

        public void StartDialogue() {
            OnDialogueStarted?.Invoke();
        }
    }
}
