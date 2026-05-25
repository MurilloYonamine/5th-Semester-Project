using System;
using FifthSemester.Core.Enums;
using FifthSemester.Core.Services;
using FifthSemester.Features.Localization;
using FifthSemester.Gameplay.Shared;
using UnityEngine;

namespace FifthSemester.Doors {
    public class DoorMissionInteractionAdapter : MonoBehaviour, IDeferredInteractionCompletion {
        [SerializeField] private LocalizedTextAsset _dialogueFiles;
        [SerializeField] private string _dialogueSourceId;

        private IDialogueService<TextAsset> _dialogueService;
        private ISettingsService _settingsService;
        private bool _hasPendingDeferredCompletion;

        public bool PublishInteractionOnInput => !_hasPendingDeferredCompletion;

        private void Start() {
            _dialogueService = ServiceLocator.Get<IDialogueService<TextAsset>>();
            _settingsService = ServiceLocator.Get<ISettingsService>();
        }

        public bool TryHandleDoorInteraction() {
            if (_hasPendingDeferredCompletion) {
                return true;
            }

            if (_dialogueFiles.Portuguese == null && _dialogueFiles.English == null) {
                return false;
            }

            if (_dialogueService == null || _settingsService == null) {
                return false;
            }

            if (_dialogueService.IsDialogueActive) {
                return true;
            }

            Language currentLanguage = _settingsService.Language;
            TextAsset dialogue = _dialogueFiles.GetAsset(currentLanguage);

            if (dialogue == null) {
                return false;
            }

            _hasPendingDeferredCompletion = true;
            _dialogueService.StartDialogue(dialogue, null, GetSourceId());
            return true;
        }

        public bool TryCompleteDeferredInteraction(string sourceId) {
            if (!_hasPendingDeferredCompletion) {
                return false;
            }

            if (!string.Equals(sourceId, GetSourceId(), StringComparison.Ordinal)) {
                return false;
            }

            _hasPendingDeferredCompletion = false;
            return true;
        }

        private string GetSourceId() {
            return string.IsNullOrWhiteSpace(_dialogueSourceId) ? gameObject.name : _dialogueSourceId;
        }
    }
}