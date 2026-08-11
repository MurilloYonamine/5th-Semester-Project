using FifthSemester.Shared;

using TMPro;
using UnityEngine;
using UnityEngine.Playables;
using ThirdParty.QuickOutline;
using FifthSemester.Core.Services;

namespace FifthSemester.Gameplay {
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

        [Header("Timeline")]
        private PlayableDirector _director;

        private Outline _outline;
        private IAudioService _audioService;
        private bool _isCompleted;
        private bool _hasPlayedDirector;

        public bool HasPlayedDeliveryCutscene => _hasPlayedDirector;

        public bool IsInteractable => !_isCompleted;

        private void Awake() {
            _outline = GetComponent<Outline>();
            _director = GetComponent<PlayableDirector>();
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

            PlayDirectorIfPasswordComplete();
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

        private void PlayDirectorIfPasswordComplete() {
            if (_hasPlayedDirector || _director == null || _passwordController == null || !_passwordController.IsComplete) {
                return;
            }

            _hasPlayedDirector = true;
            _director.time = 0d;
            _director.Play();
        }
    }
}