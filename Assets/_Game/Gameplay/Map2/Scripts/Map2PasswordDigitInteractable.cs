using FifthSemester.Gameplay.Shared;
using FifthSemester.Gameplay.Dialogue;
using FifthSemester.Core.Enums;
using FifthSemester.Core.Services;
using FifthSemester.Features.Localization;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine;

namespace FifthSemester.Gameplay.Map2 {
    [RequireComponent(typeof(Collider))]
    [RequireComponent(typeof(SpriteRenderer))]
    public class Map2PasswordDigitInteractable : MonoBehaviour, IInteractable, IPointerEnterHandler, IPointerExitHandler {
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
        private Collider _collider;
        [Header("Highlight")]
        [SerializeField] private Color _hoverColor = new Color32(255, 100, 100, 255);

        private Color _defaultColor = Color.white;
        private Coroutine _hideCaptionRoutine;
        private bool _hasInteracted;

        public string Id => gameObject.name;

        public bool IsInteractable => !_hasInteracted && _passwordController != null && _passwordController.CanRevealDigit(_digit);

        private void Awake() {
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _collider = GetComponent<Collider>();
            if (_spriteRenderer != null) {
                _defaultColor = _spriteRenderer.color;
            }
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
            if (_hasInteracted || _passwordController == null) {
                return;
            }

            bool revealed = _passwordController.TryRevealDigit(_digit);
            if (!revealed) {
                return;
            }

            _hasInteracted = true;
            if (_collider != null) {
                _collider.enabled = false;
            }

            Highlight(false);

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
            if (_spriteRenderer == null) return;

            _spriteRenderer.color = value ? _hoverColor : _defaultColor;
        }

        public void OnPointerEnter(PointerEventData eventData) {
            if (_spriteRenderer == null) return;
            _spriteRenderer.color = _hoverColor;
        }

        public void OnPointerExit(PointerEventData eventData) {
            if (_spriteRenderer == null) return;
            _spriteRenderer.color = _defaultColor;
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
