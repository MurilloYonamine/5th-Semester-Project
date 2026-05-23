// Autor: Murillo Gomes Yonamine
// Data: 23/05/2026

using FifthSemester.Core.Enums;
using FifthSemester.Core.Services;
using FifthSemester.Features.Localization;
using FifthSemester.Gameplay.Shared;
using ThirdParty.QuickOutline;
using UnityEngine;

namespace FifthSemester.Gameplay.Dialogue {
    [RequireComponent(typeof(Outline))]
    public class DocumentTrigger : MonoBehaviour, IInteractable {
        [SerializeField] private DocumentReaderView _documentReader;
        private ISettingsService _settingsService;
        private Outline _outline;

        [Header("Configuração da Carta")]
        [SerializeField] private LocalizedTextAsset _documentFiles;

        public bool IsInteractable => _documentFiles.Portuguese != null || _documentFiles.English != null;

        public string Id { get; private set; }

        private bool _documentOpen;

        private void Awake() {
            _outline = GetComponent<Outline>();
            _outline.enabled = false;
        }

        private void Start() {
            _settingsService = ServiceLocator.Get<ISettingsService>();
        }

        public void Interact() {
            if (_documentReader == null) {
                Debug.LogWarning("[DocumentTrigger] DocumentReader não atribuído!");
                return;
            }

            if (_documentOpen) {
                _documentReader.CloseDocument();
                _documentOpen = false;
                _outline.enabled = true;
                return;
            }

            var currentLanguage = _settingsService != null ? _settingsService.Language : Language.Portuguese;

            TextAsset correctFile = _documentFiles.GetAsset(currentLanguage);

            if (correctFile != null) {
                _outline.enabled = false;
                _documentReader.OpenDocument(correctFile);
                _documentOpen = true;
            }
            else {
                Debug.LogWarning($"[DocumentTrigger] Ficheiro não encontrado para o idioma: {currentLanguage}");
            }
        }

        public void StopInteract() {
            if (_documentReader != null && _documentOpen) {
                _documentReader.CloseDocument();
                _documentOpen = false;
            }
        }

        public void Highlight(bool value) {
            _outline.enabled = value;
        }
    }
}
