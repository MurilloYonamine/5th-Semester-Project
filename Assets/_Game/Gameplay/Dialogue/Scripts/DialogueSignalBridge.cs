using FifthSemester.Core.Events;
using FifthSemester.Core.Services;
using FifthSemester.Gameplay.Dialogue;
using UnityEngine;

namespace FifthSemester.Gameplay {
    public class DialogueSignalBridge : MonoBehaviour {

        [SerializeField] private Animator _npcAnimator;
        private string _talkingParameter = "IsTalking";

        private string _speedParameter = "Speed";

<<<<<<< HEAD
        [Header("Fallback Idle")]
        [Tooltip("Opcional: nome do estado Idle no Animator. Se preenchido, o bridge fará Play(nome) ao terminar diálogo.")]
        [SerializeField] private string _idleStateName = string.Empty;

        [Tooltip("Se true, forçar retorno ao estado inicial do Animator quando não houver IdleStateName definido (uses Animator.Rebind()).")]
        [SerializeField] private bool _forceRebindIfNoIdle = true;

=======
>>>>>>> origin/main
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
<<<<<<< HEAD
            _npcAnimator.SetFloat(_speedParameter, 0f);
=======
>>>>>>> origin/main
        }

        private void OnDialogueEnded(DialogueEndedEvent evt) {
            if (_npcAnimator == null) return;

<<<<<<< HEAD
            // Parar fala e zerar velocidade
            _npcAnimator.SetBool(_talkingParameter, false);
            _npcAnimator.SetFloat(_speedParameter, 0f);

            // Forçar retorno ao estado de idle: Play(nome) se fornecido, senão Rebind() opcional.
            if (!string.IsNullOrWhiteSpace(_idleStateName)) {
                try {
                    _npcAnimator.Play(_idleStateName, 0, 0f);
                }
                catch {
                    if (_forceRebindIfNoIdle) _npcAnimator.Rebind();
                }
            }
            else if (_forceRebindIfNoIdle) {
                _npcAnimator.Rebind();
            }
=======
            _npcAnimator.SetBool(_talkingParameter, false);
>>>>>>> origin/main
        }
    }
}
