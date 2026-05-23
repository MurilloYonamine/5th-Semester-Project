// Autor: Murillo Gomes Yonamine
// Data: 23/05/2026

using UnityEngine;
using TMPro;
using FifthSemester.Core.Services;
using FifthSemester.Core.States;
using FifthSemester.Core.Events;

namespace FifthSemester.Gameplay.Dialogue {

    public class DocumentReaderView : MonoBehaviour {
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI _titleText;
        [SerializeField] private TextMeshProUGUI _bodyText;
        [SerializeField] private TextMeshProUGUI _pageCounterText; 

        [Header("Audio")]
        private IAudioService _audioService;
        [SerializeField] private AudioClip _turnPageSfx;
        [SerializeField] private AudioClip _openCloseSfx;

        [SerializeField] private CanvasGroup _canvasGroup;
        private string[] _pages;
        private int _currentPage = 0;
        private IGameStateService _gameState;
        private IEventBus _eventBus;
        private bool _isOpen;

        private void Start() {
            _audioService = ServiceLocator.Get<IAudioService>();
            _gameState = ServiceLocator.Get<IGameStateService>();
            _eventBus = ServiceLocator.Get<IEventBus>();

            if (_eventBus != null) {
                _eventBus.Subscribe<InteractInputEvent>(OnInteractInput);
            }

            Hide();
        }

        private void OnEnable() {
        }

        private void OnDisable() {
            if (_eventBus != null) {
                _eventBus.Unsubscribe<InteractInputEvent>(OnInteractInput);
            }
        }

        public void OpenDocument(TextAsset documentFile) {
            DocumentData data = DocumentParser.Parse(documentFile);

            _titleText.text = data.Title;
            _pages = data.Pages;
            _currentPage = 0;

            UpdateUI();

            _gameState.ChangeState(GameState.Dialogue);

            if (_openCloseSfx != null)
                _audioService.PlaySFX(_openCloseSfx);

            _isOpen = true;
            Show();
        }

        public void CloseDocument() {
            Hide();
            _isOpen = false;

            if (_openCloseSfx != null)
                _audioService.PlaySFX(_openCloseSfx);

            _gameState.ChangeState(GameState.Gameplay);
        }

        public void NextPage() {
            if (_pages == null || _currentPage >= _pages.Length - 1) return;

            _currentPage++;
            PlayTurnSound();
            UpdateUI();
        }

        public void PreviousPage() {
            if (_pages == null || _currentPage <= 0) return;

            _currentPage--;
            PlayTurnSound();
            UpdateUI();
        }


        private void UpdateUI() {
            _bodyText.text = _pages[_currentPage].Trim();

            if (_pageCounterText != null) {
                _pageCounterText.text = $"{_currentPage + 1} / {_pages.Length}";
            }
        }

        private void PlayTurnSound() {
            if (_turnPageSfx != null) {
                _audioService.PlaySFX(_turnPageSfx);
            }
        }

        private void Show() {
            _canvasGroup.alpha = 1f;
            _canvasGroup.interactable = true;
            _canvasGroup.blocksRaycasts = true;
        }

        private void Hide() {
            _canvasGroup.alpha = 0f;
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;
        }

        private void OnInteractInput(InteractInputEvent evt) {
            if (_isOpen) {
                CloseDocument();
            }
        }
    }
}
