// Autor: Murillo Gomes Yonamine
// Data: 23/05/2026

using FifthSemester.Core.Enums;
using FifthSemester.Core.Services;
using FifthSemester.Features.Localization;
using ThirdParty.QuickOutline;
using UnityEngine;
using FifthSemester.Core.Events;

namespace FifthSemester.Gameplay.Dialogue {
    [RequireComponent(typeof(Outline))]
    public class DocumentTrigger : TextTriggerBase {
        [SerializeField] private DocumentView _documentView;
        private ISettingsService _settingsService;
        private IEventBus _eventBus;

        [Header("Configuração da Carta")]
        [SerializeField] private LocalizedTextAsset _documentFiles;

        public override bool IsInteractable => _documentFiles.Portuguese != null || _documentFiles.English != null;

        private bool _documentOpen;

        protected override void Awake() {
            base.Awake();
        }

        protected virtual void Start() {
            _settingsService = ServiceLocator.Get<ISettingsService>();

            if (_settingsService == null) {
                Debug.LogError($"[DocumentTrigger] ISettingsService não encontrado em {name}.");
                enabled = false;
                return;
            }

            if (_documentView == null) {
                Debug.LogError($"[DocumentTrigger] DocumentView não atribuído em {name}.");
                enabled = false;
                return;
            }

            _eventBus = ServiceLocator.Get<IEventBus>();
            if (_eventBus == null) {
                Debug.LogError($"[DocumentTrigger] IEventBus não encontrado em {name}.");
                enabled = false;
                return;
            }
        }

        protected override void OnInteract() {
            if (_documentFiles.Portuguese == null && _documentFiles.English == null) {
                Debug.LogError($"[DocumentTrigger] Nenhum texto de documento configurado em {name}.");
                return;
            }

            if (_documentOpen) {
                _documentView.Hide();
                _documentOpen = false;
                Highlight(true);
                _eventBus?.Publish(new DialogueEndedEvent { NpcId = null });
                return;
            }

            Language currentLanguage = _settingsService.Language;

            TextAsset correctFile = _documentFiles.GetAsset(currentLanguage);

            if (correctFile == null) {
                Debug.LogWarning($"[DocumentTrigger] Ficheiro não encontrado para o idioma: {currentLanguage}");
                return;
            }

            DocumentData documentData = DocumentParser.Parse(correctFile);

            Highlight(false);
            _documentView.SetDocument(documentData);
            _documentView.Show();
            _documentOpen = true;
            _eventBus?.Publish(new DialogueStartedEvent());
        }

        public override void StopInteract() {
            if (_documentOpen) {
                _documentView.Hide();
                _documentOpen = false;
                _eventBus?.Publish(new DialogueEndedEvent { NpcId = null });
            }
        }

        public override void Highlight(bool value) {
            base.Highlight(value);
        }
    }
}
