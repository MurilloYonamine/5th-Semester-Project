using FifthSemester.Core.Events;
using FifthSemester.Core.Services;
using FifthSemester.Gameplay.Dialogue;
using UnityEngine;

namespace FifthSemester.Gameplay {
    public class DialogueSignalBridge : MonoBehaviour {

        [SerializeField] private Animator _npcAnimator;
        private string _talkingParameter = "IsTalking";

        private string _speedParameter = "Speed";

        private IDialogueService<TextAsset> _dialogueService;
        private IEventBus _eventBus;

        private void Start() {
            _dialogueService = ServiceLocator.Get<IDialogueService<TextAsset>>();
            _eventBus = ServiceLocator.Get<IEventBus>();

            _eventBus?.Subscribe<DialogueAdvanceRequestedEvent>(OnDialogueAdvanced);
            _eventBus?.Subscribe<DialogueEndedEvent>(OnDialogueEnded);
        }

        private void OnDestroy() {
            _eventBus?.Unsubscribe<DialogueAdvanceRequestedEvent>(OnDialogueAdvanced);
            _eventBus?.Unsubscribe<DialogueEndedEvent>(OnDialogueEnded);
        }

        public void TriggerWalk(float speed) {
            if (_npcAnimator != null) {
                _npcAnimator.SetFloat(_speedParameter, speed);
            }
        }

        public void TriggerNextLine() {
            if (_npcAnimator != null) {
                _npcAnimator.SetFloat(_speedParameter, 0f);

                _npcAnimator.SetBool(_talkingParameter, true);
            }

            _dialogueService.TimelineShowLine();
        }

        private void OnDialogueAdvanced(DialogueAdvanceRequestedEvent evt) {
            if (_npcAnimator == null) return;

            _npcAnimator.SetBool(_talkingParameter, false);
        }

        private void OnDialogueEnded(DialogueEndedEvent evt) {
            if (_npcAnimator == null) return;

            _npcAnimator.SetBool(_talkingParameter, false);
        }
    }
}
