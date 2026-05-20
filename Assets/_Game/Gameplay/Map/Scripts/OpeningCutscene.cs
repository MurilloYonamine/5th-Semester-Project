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

namespace FifthSemester.Gameplay.Dialogue {
    public class OpeningCutscene : MonoBehaviour {

        [SerializeField, Title("Textos da Cutscene Inicial")]
        private LocalizedTextAsset _dialogueFiles;

        [SerializeField, Title("Timeline (Director)")]
        private PlayableDirector _director;

        public void PlayCutscene() {
            IGameStateService gameStateService = ServiceLocator.Get<IGameStateService>();
            IDialogueService<TextAsset> dialogueService = ServiceLocator.Get<IDialogueService<TextAsset>>();
            ISettingsService settingsService = ServiceLocator.Get<ISettingsService>();

            if (dialogueService == null || gameStateService == null) return;

            Language currentLanguage = settingsService != null ? settingsService.Language : Language.Portuguese;
            TextAsset correctDialogue = _dialogueFiles.GetAsset(currentLanguage);

            if (correctDialogue == null) {
                Debug.LogWarning("[Cutscene] Nenhum arquivo TXT de abertura encontrado!");
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
        }

        private void OnCutsceneStopped(PlayableDirector director) {
            IGameStateService gameStateService = ServiceLocator.Get<IGameStateService>();

            if (_director != null) {
                _director.stopped -= OnCutsceneStopped;
            }

            gameStateService?.ChangeState(GameState.Gameplay);
        }
    }
}
