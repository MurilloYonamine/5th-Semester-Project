// autor: Murillo Gomes Yonamine
// data: 24/05/2026

using FifthSemester.Core.Enums;
using FifthSemester.Core.Services;
using FifthSemester.Features.Localization;
using UnityEngine;

namespace FifthSemester.Gameplay.Dialogue {
    [RequireComponent(typeof(Collider))]
    public class CaptionTrigger : TextTriggerBase {
        [Header("Caption")]
        [SerializeField] private LocalizedTextAsset _captionFiles;
        [SerializeField] private CaptionView _captionView;
        private ISettingsService _settingsService;

        public override bool IsInteractable => _captionFiles.Portuguese != null || _captionFiles.English != null;

        protected override void Awake() {
            base.Awake();
        }

        protected virtual void Start() {
            _settingsService = ServiceLocator.Get<ISettingsService>();

            if (_settingsService == null) {
                Debug.LogError($"[CaptionTrigger] ISettingsService não encontrado em {name}.");
                enabled = false;
                return;
            }

            if (_captionView == null) {
                Debug.LogError($"[CaptionTrigger] CaptionView não atribuído em {name}.");
                enabled = false;
                return;
            }
        }

        protected override void OnInteract() {
            if (_captionFiles.Portuguese == null && _captionFiles.English == null) {
                Debug.LogError($"[CaptionTrigger] Nenhum texto de legenda configurado em {name}.");
                return;
            }

            Language currentLanguage = _settingsService.Language;
            TextAsset captionFile = _captionFiles.GetAsset(currentLanguage);

            if (captionFile == null) {
                Debug.LogWarning($"[CaptionTrigger] Nenhum ficheiro TXT encontrado para o idioma {currentLanguage} em {name}.");
                return;
            }

            _captionView.Show();
            _captionView.SetCaption(CaptionParser.Parse(captionFile));
        }

        public override void StopInteract() {
            if (_captionView != null) {
                _captionView.Hide();
            }
        }

        public override void Highlight(bool value) {
            base.Highlight(value);
        }
    }
}