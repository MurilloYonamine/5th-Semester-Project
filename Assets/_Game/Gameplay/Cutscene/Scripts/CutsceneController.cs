// autor: Murillo Gomes Yonamine
// data: 18/05/2026

using FifthSemester.Core.Enums;
using FifthSemester.Core.Events; 
using FifthSemester.Core.Services;
using FifthSemester.Core.States;
using FifthSemester.Features.Localization;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.InputSystem;
using Unity.Cinemachine;

namespace FifthSemester.Gameplay.Dialogue {
    public class CutsceneController : MonoBehaviour {

        [SerializeField] public CutsceneType CutsceneID = CutsceneType.OpeningCutscene;

        [SerializeField, Title("Textos da Cutscene Inicial")]
        private LocalizedTextAsset _dialogueFiles;

        [SerializeField, Title("Timeline (Director)")]
        private PlayableDirector _director;

        [SerializeField, Tooltip("Allow the player to skip the opening cutscene")]
        private bool _allowSkip = true;

        private bool _isPlaying;
        private CinemachineCamera _playerCamera;

        [SerializeField, Tooltip("Animator que deve voltar para idle ao fim/skip da cutscene")]
        private Animator _targetAnimator;

        private readonly int _speedHash = Animator.StringToHash("Speed");

        private IEventBus _eventBus;

        private void OnEnable() {
            _eventBus = ServiceLocator.Get<IEventBus>();
            if (_eventBus != null) {
                _eventBus.Subscribe<SkipCutsceneRequestedEvent>(OnSkipCutsceneRequested);
            }
        }

        private void OnDisable() {
            if (_eventBus != null) {
                _eventBus.Unsubscribe<SkipCutsceneRequestedEvent>(OnSkipCutsceneRequested);
            }
        }

        public void SetPlayerCamera(CinemachineCamera playerCamera) {
            _playerCamera = playerCamera;
        }

        public void PlayCutscene() {
            IGameStateService gameStateService = ServiceLocator.Get<IGameStateService>();
            IDialogueService<TextAsset> dialogueService = ServiceLocator.Get<IDialogueService<TextAsset>>();
            ISettingsService settingsService = ServiceLocator.Get<ISettingsService>();

            if (dialogueService == null || gameStateService == null) return;

            Language currentLanguage = settingsService != null ? settingsService.Language : Language.Portuguese;
            TextAsset correctDialogue = _dialogueFiles.GetAsset(currentLanguage);

            if (correctDialogue == null) {
                return;
            }

            gameStateService.ChangeState(GameState.Cutscene);

            if (_director != null) {
                _director.stopped -= OnCutsceneStopped;
                _director.stopped += OnCutsceneStopped;
                _director.time = 0d;
                _director.Evaluate();
                _director.Play();
            }

            dialogueService.StartDialogue(correctDialogue, _director);

            _isPlaying = true;
        }

        private void OnSkipCutsceneRequested(SkipCutsceneRequestedEvent evt) {
            if (!_isPlaying) return;
            SkipCutscene();
        }

        public void SkipCutscene() {
            if (_director != null) {
                _director.stopped -= OnCutsceneStopped;
                _director.time = _director.duration;
                _director.Evaluate();
                _director.Stop();
            }

            RestorePlayerCamera();

            IGameStateService gameStateService = ServiceLocator.Get<IGameStateService>();
            gameStateService?.ChangeState(GameState.Gameplay);

            _isPlaying = false;
        }

        private void OnCutsceneStopped(PlayableDirector director) {
            if (_director != null) {
                _director.stopped -= OnCutsceneStopped;
            }

            RestorePlayerCamera();

            IGameStateService gameStateService = ServiceLocator.Get<IGameStateService>();
            gameStateService?.ChangeState(GameState.Gameplay);

            _isPlaying = false;
        }

        private void RestorePlayerCamera() {
            if (_playerCamera != null) {
                _playerCamera.Priority = 1;
            }

            if (_targetAnimator != null) {
                _targetAnimator.SetFloat(_speedHash, 0f);
            }
        }
    }
}
