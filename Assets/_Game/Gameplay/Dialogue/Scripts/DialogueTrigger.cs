// autor: Murillo Gomes Yonamine
// data: 30/03/2026

using FifthSemester.Core.Enums;
using FifthSemester.Core.Services;
using FifthSemester.Features.Localization;
using FifthSemester.Gameplay.Shared;
using Sirenix.OdinInspector;
using ThirdParty.QuickOutline;
using UnityEngine;
using UnityEngine.Playables;

namespace FifthSemester.Gameplay.Dialogue {
    [RequireComponent(typeof(Outline))]
    public class DialogueTrigger : MonoBehaviour, IInteractable {
        private IDialogueService<TextAsset> _dialogueService;
        private ISettingsService _settingsService;

        [field: SerializeField] public string Id { get; private set; }

        [SerializeField, Title("Textos do Diálogo")]
        private LocalizedTextAsset _dialogueFiles;

        private Outline _outline;

        public bool IsInteractable => _dialogueFiles.Portuguese != null || _dialogueFiles.English != null;

        [SerializeField] private PlayableDirector _director;

        private void Awake() {
            _outline = GetComponent<Outline>();
            _outline.enabled = false;
        }

        private void Start() {
            _dialogueService = ServiceLocator.Get<IDialogueService<TextAsset>>();
            _settingsService = ServiceLocator.Get<ISettingsService>();
        }

        public void Interact() {
            Language currentLanguage = _settingsService != null ? _settingsService.Language : Language.Portuguese;

            TextAsset correctDialogue = _dialogueFiles.GetAsset(currentLanguage);

            if (correctDialogue == null) {
                Debug.LogWarning($"[Dialogue] Nenhum ficheiro TXT encontrado para o idioma {currentLanguage} no NPC {gameObject.name}!");
                return;
            }

            string dialogueId = string.IsNullOrWhiteSpace(Id) ? gameObject.name : Id;
            _dialogueService.StartDialogue(correctDialogue, _director, dialogueId);
        }

        public void StopInteract() {
            _dialogueService.EndDialogue();
        }

        public void Highlight(bool value) {
            _outline.enabled = value;
        }
    }
}
