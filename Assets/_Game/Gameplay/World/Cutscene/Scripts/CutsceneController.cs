// autor: Murillo Gomes Yonamine
// data: 21/05/2026

using FifthSemester.Core.Enums;
using FifthSemester.Core.Events;
using FifthSemester.Core.Services;
using FifthSemester.Core.States;

using Sirenix.OdinInspector;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Playables;

namespace FifthSemester.Gameplay {
    public class CutsceneController : MonoBehaviour {

        [SerializeField, Title("Identificação")]
        public CutsceneType CutsceneID = CutsceneType.OpeningCutscene;

        [SerializeField, Title("Configurações")]
        [Tooltip("Marque se esta cutscene possui texto/diálogo.")]
        private bool _hasDialogue = false;

        [SerializeField, Title("Textos da Cutscene")]
        [ShowIf("_hasDialogue")] 
        private LocalizedTextAsset _dialogueFiles;

        [SerializeField, Title("Timeline (Director)")]
        private PlayableDirector _director;

        [SerializeField, Tooltip("Permite o jogador pular esta cutscene")]
        private bool _allowSkip = true;

        [SerializeField, Tooltip("Animator que deve voltar para idle ao fim/skip da cutscene")]
        private Animator _targetAnimator;

        private bool _isPlaying;
        private CinemachineCamera _playerCamera;
        private readonly int _speedHash = Animator.StringToHash("Speed");

        public bool IsPlaying => _isPlaying;

        public void SetPlayerCamera(CinemachineCamera playerCamera) {
            _playerCamera = playerCamera;
        }

        public void PlayCutscene() {
            IGameStateService gameStateService = ServiceLocator.Get<IGameStateService>();

            if (gameStateService == null)
                return;

            gameStateService.ChangeState(GameState.Cutscene);

            if (_director != null) {
                _director.stopped -= OnCutsceneStopped;
                _director.stopped += OnCutsceneStopped;

                _director.time = 0d;
                _director.Evaluate();
                _director.Play();
            }

            if (_hasDialogue && _dialogueFiles.Portuguese != null || _dialogueFiles.English != null) {
                IDialogueService<TextAsset> dialogueService = ServiceLocator.Get<IDialogueService<TextAsset>>();
                ISettingsService settingsService = ServiceLocator.Get<ISettingsService>();

                if (dialogueService != null) {
                    Language currentLanguage = settingsService != null ? settingsService.Language : Language.Portuguese;
                    TextAsset correctDialogue = _dialogueFiles.GetAsset(currentLanguage);

                    if (correctDialogue != null) {
                        dialogueService.StartDialogue(correctDialogue, _director, null, DialogueMode.Cutscene);
                    }
                    else {
                        Debug.LogWarning($"Cutscene {CutsceneID} está marcada para ter diálogo, mas faltam arquivos de texto!");
                    }
                }
            }

            _isPlaying = true;
        }

        public void SkipCutscene() {
            if (!_isPlaying || !_allowSkip)
                return;

            if (_director != null) {
                _director.stopped -= OnCutsceneStopped;

                _director.time = _director.duration;
                _director.Evaluate();
                _director.Stop();
            }

            FinishCutscene();
        }

        private void OnCutsceneStopped(PlayableDirector director) {
            if (_director != null) {
                _director.stopped -= OnCutsceneStopped;
            }

            FinishCutscene();
        }

        private void FinishCutscene() {
            if (_hasDialogue) {
                EndCutsceneDialogue();
            }

            RestorePlayerCamera();

            IGameStateService gameStateService = ServiceLocator.Get<IGameStateService>();
            gameStateService?.ChangeState(GameState.Gameplay);

            _isPlaying = false;

            IEventBus eventBus = ServiceLocator.Get<IEventBus>();
            eventBus?.Publish(new CutsceneEndedEvent { CutsceneID = CutsceneID });
        }

        private void EndCutsceneDialogue() {
            IDialogueService<TextAsset> dialogueService = ServiceLocator.Get<IDialogueService<TextAsset>>();
            dialogueService?.EndDialogue();
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
