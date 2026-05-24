// autor: Murillo Gomes Yonamine
// data: 30/03/2026

using FifthSemester.Core.Enums;
using FifthSemester.Core.Services;
using FifthSemester.Features.Localization;
using Sirenix.OdinInspector;
using ThirdParty.QuickOutline;
using UnityEngine;
using UnityEngine.Playables;

namespace FifthSemester.Gameplay.Dialogue {
    [RequireComponent(typeof(Outline))]
    public class DialogueTrigger : TextTriggerBase {
        private IDialogueService<TextAsset> _dialogueService;
        private ISettingsService _settingsService;

        [SerializeField, Title("Textos do Diálogo")]
        private LocalizedTextAsset _dialogueFiles;

        public override bool IsInteractable => _dialogueFiles.Portuguese != null || _dialogueFiles.English != null;

        [SerializeField] private PlayableDirector _director;

        protected override void Awake() {
            base.Awake();
        }

        protected virtual void Start() {
            _dialogueService = ServiceLocator.Get<IDialogueService<TextAsset>>();
            _settingsService = ServiceLocator.Get<ISettingsService>();

            if (_dialogueService == null) {
                Debug.LogError($"[DialogueTrigger] IDialogueService<TextAsset> não encontrado em {name}.");
                enabled = false;
                return;
            }

            if (_settingsService == null) {
                Debug.LogError($"[DialogueTrigger] ISettingsService não encontrado em {name}.");
                enabled = false;
                return;
            }
        }

        protected override bool CanInteract() {
            return _dialogueService != null && !_dialogueService.IsDialogueActive && IsInteractable;
        }

        protected override void OnInteract() {
            if (_dialogueFiles.Portuguese == null && _dialogueFiles.English == null) {
                Debug.LogError($"[DialogueTrigger] Nenhum texto de diálogo configurado em {name}.");
                return;
            }

            Language currentLanguage = _settingsService != null ? _settingsService.Language : Language.Portuguese;

            TextAsset correctDialogue = _dialogueFiles.GetAsset(currentLanguage);

            if (correctDialogue == null) {
                Debug.LogWarning($"[Dialogue] Nenhum ficheiro TXT encontrado para o idioma {currentLanguage} no NPC {gameObject.name}!");
                return;
            }

            Highlight(false);

            string dialogueId = string.IsNullOrWhiteSpace(Id) ? gameObject.name : Id;
            _dialogueService.StartDialogue(correctDialogue, _director, dialogueId);
        }

        public override void StopInteract() {
            _dialogueService.EndDialogue();
        }

        public override void Highlight(bool value) {
            bool isDialogueActive = _dialogueService != null && _dialogueService.IsDialogueActive;
            base.Highlight(!isDialogueActive && value);
        }
    }
}
