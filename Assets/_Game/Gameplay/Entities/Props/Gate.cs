using FifthSemester.Shared;
using System.Collections;
using FifthSemester.Core.Enums;
using FifthSemester.Core.Services;



using Sirenix.OdinInspector;
using ThirdParty.QuickOutline;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace FifthSemester.Gameplay {
    [RequireComponent(typeof(Collider))]
    public class Gate : MonoBehaviour, IInteractable {
        [Header("Identity")]
        [SerializeField] private string _id;

        [Header("Visual Feedback")]
        [SerializeField] private Outline _outline;
        [SerializeField] private GameObject _interactionHint;
        [SerializeField] private TMP_Text _interactionText;

        [Header("Narrative")]
        [SerializeField] private LocalizedTextAsset _captionFiles;
        [SerializeField] private GateTransitionView _transitionView;
        [SerializeField] private string _nextSceneName = "Game_Mapa2";
        [SerializeField, Min(0f)] private float _fadeDuration = 1f;
        [SerializeField, Min(0f)] private float _postTextHoldDuration = 5f;
        [SerializeField] private bool _startLocked = true;

        [Header("Locked State")]
        [SerializeField] private string _lockedText = "TRANCADO";
        [SerializeField] private Color _lockedColor = Color.red;
        [SerializeField] private Color _lockedOutlineColor = Color.red;
        [SerializeField] private bool _showOutlineWhenLocked = false;

        private ISettingsService _settingsService;
        private IFadeService _fadeService;
        private IMapService _mapService;
        private string _defaultText;
        private Color _defaultColor;
        private Color _defaultOutlineColor;
        private bool _isHighlighted;
        private bool _sequenceRunning;
        private bool _isUsed;
        private bool _isLocked;

        public string Id => string.IsNullOrWhiteSpace(_id) ? gameObject.name : _id;

        public bool IsInteractable => !_isLocked && !_isUsed && !_sequenceRunning && _transitionView != null && HasCaptionText();

        private void Awake() {
            if (_outline == null && !TryGetComponent(out _outline)) {
                _outline = null;
            }

            if (_interactionText == null) {
                _interactionText = GetComponentInChildren<TMP_Text>(true);
            }

            if (_interactionText != null) {
                _defaultText = _interactionText.text;
                _defaultColor = _interactionText.color;
            }

            if (_outline != null) {
                _defaultOutlineColor = _outline.OutlineColor;
            }

            bool transitionAssigned = _transitionView != null;
            bool captionFilesSet = HasCaptionText();
            bool outlineAssigned = _outline != null;

            if (_transitionView == null) {
                Debug.LogError($"[Gate] Transition view is not assigned on {name}.");
                enabled = false;
                return;
            }

            ApplyLockState(_startLocked);
            TryRegisterGate();
            Highlight(false);
        }

        private void Start() {
            _settingsService = ServiceLocator.Get<ISettingsService>();
            ServiceLocator.TryGet<IFadeService>(out _fadeService);
            TryRegisterGate();
        }

        private void OnDestroy() {
            if (_mapService != null) {
                _mapService.Unregister(Id);
            }
        }

        public void Interact() {
            if (!CanStartSequence()) {
                return;
            }

            _isUsed = true;
            _sequenceRunning = true;
            Highlight(false);

            if (_fadeService == null) {
                ServiceLocator.TryGet<IFadeService>(out _fadeService);
            }

            if (_fadeService == null) {
                PlayCaptionAndLoadScene();
                return;
            }

            _fadeService.FadeOut(_fadeDuration, PlayCaptionAndLoadScene);
        }

        public void StopInteract() {
            Highlight(false);

            if (_transitionView != null) {
                _transitionView.ClearMessage();
            }
        }

        public void Highlight(bool value) {
            _isHighlighted = value;
            UpdateVisualState();
        }

        public void Lock() {
            ApplyLockState(true);
        }

        public void Unlock() {
            ApplyLockState(false);
        }

        private void ApplyLockState(bool locked) {
            _isLocked = locked;
            UpdateVisualState();
        }

        private void UpdateVisualState() {
            bool outlineEnabled = (_isHighlighted && IsInteractable) || (_isLocked && _showOutlineWhenLocked);

            if (_outline != null) {
                _outline.enabled = outlineEnabled;
                _outline.OutlineColor = _isLocked ? _lockedOutlineColor : _defaultOutlineColor;
            }

            bool hintActive = _isHighlighted && IsInteractable;
            if (_interactionHint != null) {
                _interactionHint.SetActive(hintActive);
            }

            if (_interactionText != null) {
                if (_isLocked) {
                    _interactionText.text = _lockedText;
                    _interactionText.color = _lockedColor;
                }
                else {
                    _interactionText.text = _defaultText;
                    _interactionText.color = _defaultColor;
                }
            }

        }

        private void TryRegisterGate() {
            if (_mapService == null) {
                _mapService = ServiceLocator.Get<IMapService>();
            }

            if (_mapService == null) {
                return;
            }

            _mapService.Register(Id, gameObject);
        }


        private bool CanStartSequence() {
            return IsInteractable && _transitionView != null && HasCaptionText();
        }

        private bool HasCaptionText() {
            return (_captionFiles.Portuguese != null) || (_captionFiles.English != null);
        }

        private void PlayCaptionAndLoadScene() {
            if (_transitionView == null) {
                LoadNextScene();
                return;
            }

            if (!HasCaptionText()) {
                LoadNextScene();
                return;
            }

            Language currentLanguage = _settingsService != null ? _settingsService.Language : Language.Portuguese;
            TextAsset captionFile = _captionFiles.GetAsset(currentLanguage);

            if (captionFile == null) {
                LoadNextScene();
                return;
            }

            string message = captionFile.text != null ? captionFile.text.Trim() : string.Empty;
            _transitionView.ShowMessage(message, OnCaptionAnimationCompleted);
        }

        private void OnCaptionAnimationCompleted() {
            StartCoroutine(HoldThenLoadScene());
        }

        private IEnumerator HoldThenLoadScene() {
            if (_postTextHoldDuration > 0f) {
                yield return new WaitForSeconds(_postTextHoldDuration);
            }

            if (_transitionView != null) {
                _transitionView.ClearMessage();
            }

            LoadNextScene();
        }

        private void LoadNextScene() {
            if (string.IsNullOrWhiteSpace(_nextSceneName)) {
                Debug.LogError($"[Gate] Next scene name is empty on {name}.");
                _sequenceRunning = false;
                return;
            }

            SceneManager.LoadScene(_nextSceneName);
        }
    }
}
