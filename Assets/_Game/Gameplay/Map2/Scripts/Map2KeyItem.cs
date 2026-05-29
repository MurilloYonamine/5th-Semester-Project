using FifthSemester.Gameplay.Inventory;
using UnityEngine;
using FifthSemester.Core.Services;
using FifthSemester.Core.Enums;
using FifthSemester.Features.Localization;
using FifthSemester.Gameplay.Dialogue;
using System.Collections;

namespace FifthSemester.Gameplay.Map2 {
    public class Map2KeyItem : Item {
        [field: SerializeField] public Map2KeyDefinitionSO KeyDefinition { get; private set; }
        [SerializeField] private CaptionView _captionView;
        private IMap2KeyService _mapKeyService;

        private const float CAPTION_DURATION = 0.8f;

        protected override void Awake() {
            base.Awake();

            if (_captionView == null) {
                Debug.LogWarning($"[Map2KeyItem] CaptionView não atribuído em {name}.");
            }

            ServiceLocator.TryGet<IMap2KeyService>(out _mapKeyService);
            _mapKeyService?.RegisterKey(this);
        }

        public override void Interact() {
            base.Interact();

            if (KeyDefinition == null) return;

            LocalizedTextAsset localized = KeyDefinition.PickupDialogue;
            if (localized.Portuguese == null && localized.English == null) return;

            Language lang = Language.Portuguese;
            if (ServiceLocator.TryGet<ISettingsService>(out var settings)) {
                lang = settings.Language;
            }

            TextAsset asset = localized.GetAsset(lang);
            if (asset == null) return;

            // Prefer the assigned CaptionView so the pickup object can be disabled safely.
            if (_captionView != null) {
                _captionView.Show();
                string parsed = CaptionParser.Parse(asset);
                _captionView.SetCaption(parsed, () => {
                    _captionView.StartCoroutine(HideCaptionAfterDelay(_captionView));
                });
            }
            else {
                // fallback to dialogue system
                if (ServiceLocator.TryGet<IDialogueService<TextAsset>>(out var dialogueService)) {
                    if (dialogueService.IsDialogueActive) return;
                    dialogueService.StartDialogue(asset, null, gameObject.name);
                }
            }

            // After being picked up (PlayerInteraction publishes ItemPickedUpEvent before calling Interact), unregister from key service.
            _mapKeyService?.UnregisterKey(this);
        }

        private IEnumerator HideCaptionAfterDelay(CaptionView captionView) {
            yield return new WaitForSeconds(CAPTION_DURATION);
            if (captionView != null) captionView.Hide();
        }
    }
}
