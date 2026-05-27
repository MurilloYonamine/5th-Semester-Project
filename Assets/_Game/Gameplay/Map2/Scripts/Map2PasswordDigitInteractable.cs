using FifthSemester.Gameplay.Shared;
using FifthSemester.Gameplay.Dialogue;
using FifthSemester.Core.Enums;
using FifthSemester.Core.Services;
using FifthSemester.Features.Localization;
using System.Collections;
using ThirdParty.QuickOutline;
using UnityEngine;

namespace FifthSemester.Gameplay.Map2 {
    [RequireComponent(typeof(Collider))]
    [RequireComponent(typeof(SpriteRenderer))]
    [RequireComponent(typeof(Outline))]
    public class Map2PasswordDigitInteractable : MonoBehaviour, IInteractable {
        private const float CAPTION_DURATION = 2f;

        [Header("Configuração do Dígito")]
        [SerializeField] private int _digit;

        [Header("References")]
        [SerializeField] private Map2PasswordController _passwordController;
        [SerializeField] private CaptionView _captionView;

        [Header("Caption")]
        [SerializeField] private LocalizedTextAsset _discoverCaptionFiles;
        private ISettingsService _settingsService;

        private SpriteRenderer _spriteRenderer;
        private Outline _outline;
        [Header("Highlight")]
        [SerializeField] private Color _highlightColor = new Color32(255, 220, 120, 255);

        private Color _defaultColor = Color.white;
        private Coroutine _hideCaptionRoutine;

        public string Id => gameObject.name;

        public bool IsInteractable => _passwordController != null && _passwordController.CanRevealDigit(_digit);

        private void Awake() {
            _spriteRenderer = GetComponent<SpriteRenderer>();

            _outline = GetComponent<Outline>();
            _outline.enabled = false;

            _defaultColor = _outline.OutlineColor;
        }

        private void Start() {
            _settingsService = ServiceLocator.Get<ISettingsService>();

            if (_settingsService == null) {
                Debug.LogError($"[Map2PasswordDigitInteractable] ISettingsService não encontrado em {name}.");
                enabled = false;
                return;
            }

            if (_captionView == null) {
                Debug.LogError($"[Map2PasswordDigitInteractable] CaptionView não atribuído em {name}.");
                enabled = false;
                return;
            }
        }

        public void Interact() {
            if (_passwordController == null) {
                return;
            }

            bool revealed = _passwordController.TryRevealDigit(_digit);
            if (!revealed) {
                return;
            }

            if (_discoverCaptionFiles.Portuguese == null && _discoverCaptionFiles.English == null) {
                Debug.LogError($"[Map2PasswordDigitInteractable] Nenhum texto de legenda configurado em {name}.");
                return;
            }

            Language currentLanguage = _settingsService.Language;
            TextAsset captionFile = _discoverCaptionFiles.GetAsset(currentLanguage);

            if (captionFile == null) {
                Debug.LogWarning($"[Map2PasswordDigitInteractable] Nenhum ficheiro TXT encontrado para o idioma {currentLanguage} em {name}.");
                return;
            }

            _captionView.Show();
            StartCaptionTimerAfterTyping(CaptionParser.Parse(captionFile));
        }

        public void StopInteract() {
        }

        public void Highlight(bool value) {
            if (_spriteRenderer == null) {
                return;
            }

            _outline.enabled = value;

            _outline.OutlineColor = value ? _highlightColor : _defaultColor;
        }

        private void StartCaptionTimerAfterTyping(string captionText) {
            if (_hideCaptionRoutine != null) {
                StopCoroutine(_hideCaptionRoutine);
                _hideCaptionRoutine = null;
            }

            _captionView.SetCaption(captionText, () => {
                if (_hideCaptionRoutine != null) {
                    StopCoroutine(_hideCaptionRoutine);
                }

                _hideCaptionRoutine = StartCoroutine(HideCaptionAfterDelay());
            });
        }

        private IEnumerator HideCaptionAfterDelay() {
            yield return new WaitForSeconds(CAPTION_DURATION);

            if (_captionView != null) {
                _captionView.Hide();
            }

            _hideCaptionRoutine = null;
        }
    }
}
