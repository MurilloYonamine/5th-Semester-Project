using FifthSemester.Core.Services;
using FifthSemester.Gameplay.Dialogue;
using UnityEngine;

namespace FifthSemester.Gameplay {
    public class DialogueSignalBridge : MonoBehaviour {
        private IDialogueService<TextAsset> _dialogueService;
        private void Start() {
            _dialogueService = ServiceLocator.Get<IDialogueService<TextAsset>>();
        }
        public void TriggerNextLine() {
            _dialogueService.TimelineShowLineAndPause();
        }
    }
}
