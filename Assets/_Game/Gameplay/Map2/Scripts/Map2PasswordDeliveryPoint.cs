using FifthSemester.Gameplay.Shared;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using ThirdParty.QuickOutline;
using FifthSemester.Core.Services;

namespace FifthSemester.Gameplay.Map2 {
    [RequireComponent(typeof(Outline))]
    public class Map2PasswordDeliveryPoint : MonoBehaviour, IInteractable {
        [field: SerializeField] public string Id { get; private set; }

        [Header("References")]
        [SerializeField] private Map2PasswordController _passwordController;

        [Header("UI")]
        [SerializeField] private TextMeshPro _interactionPromptText;
        [SerializeField] private string _deliverPromptText = "entregar senha";
        [SerializeField] private string _talkPromptText = "conversar";

        [Header("Audio")]
        [SerializeField] private AudioClip _successSound;
        [SerializeField] private AudioClip _failureSound;

        [Header("Events")]
        [SerializeField] private UnityEvent _onPasswordDelivered;

        private Outline _outline;
        private IAudioService _audioService;
        private bool _isCompleted;

        public bool IsInteractable => !_isCompleted;

        private void Awake() {
            _outline = GetComponent<Outline>();
            Highlight(false);
        }

        private void Start() {
            ServiceLocator.TryGet<IAudioService>(out _audioService);
            UpdateInteractionPrompt();
            Highlight(false);
        }

        public void Highlight(bool value) {
            if (_outline != null) {
                _outline.enabled = value;
            }
        }

        public void Interact() {
            if (_isCompleted) {
                return;
            }

            if (_passwordController != null && _passwordController.IsComplete) {
                CompleteDelivery();
                UpdateInteractionPrompt();
                PlayFeedback(_successSound);
                return;
            }

            PlayFeedback(_failureSound);
        }

        public void StopInteract() {
        }

        private void CompleteDelivery() {
            _isCompleted = true;
            _onPasswordDelivered?.Invoke();
        }

        private void UpdateInteractionPrompt() {
            if (_interactionPromptText == null) {
                return;
            }

            _interactionPromptText.text = _isCompleted ? _talkPromptText : _deliverPromptText;
        }

        private void PlayFeedback(AudioClip clip) {
            if (clip == null || _audioService == null) {
                return;
            }

            _audioService.PlaySFX(clip);
        }
    }
}