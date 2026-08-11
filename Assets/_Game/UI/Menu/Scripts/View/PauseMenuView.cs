using FifthSemester.Core.Enums;
using FifthSemester.Core.Services;
using FifthSemester.Core.States;
using FifthSemester.Core.Events;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace FifthSemester.UI {
    public class PauseMenuView : MenuViewBase {
        private IGameStateService _gameState;
        private IEventBus _eventBus;

        [Header("Background Panel")]
        [SerializeField] private CanvasGroup _backgroundPanel; 

        [Header("Buttons")]
        [SerializeField] private Button _resumeButton;
        [SerializeField] private Button _loadButton;
        [SerializeField] private Button _settingsButton;
        [SerializeField] private Button _creditsButton;
        [SerializeField] private Button _quitButton;

        protected override MenuScreen MenuScreenType => MenuScreen.PauseMenu;

        protected override void Start() {
            _gameState = ServiceLocator.Get<IGameStateService>();
            _eventBus = ServiceLocator.Get<IEventBus>();

            _eventBus?.Subscribe<GameStateChangedEvent>(OnGameStateChanged);

            base.Start();

            _resumeButton.onClick.AddListener(OnResume);
            _settingsButton.onClick.AddListener(OnSettings);
            _creditsButton.onClick.AddListener(OnCredits);
            _quitButton.onClick.AddListener(OnQuit);

            if (_loadButton != null) {
                _loadButton.gameObject.SetActive(false);
            }
            
            if (_gameState.CurrentState == GameState.Gameplay && _backgroundPanel != null) {
                 _backgroundPanel.alpha = 0f;
                 _backgroundPanel.blocksRaycasts = false;
                 _backgroundPanel.interactable = false;
            }
        }

        protected override void OnDestroy() {
            base.OnDestroy();
            _eventBus?.Unsubscribe<GameStateChangedEvent>(OnGameStateChanged);
        }

        private void OnGameStateChanged(GameStateChangedEvent evt) {
            if (evt.CurrentState == GameState.Paused) {
                if (_backgroundPanel != null) {
                    _backgroundPanel.alpha = 1f;
                    _backgroundPanel.blocksRaycasts = true;
                    _backgroundPanel.interactable = true;
                }

                _menuService.Show(MenuScreen.PauseMenu);
            }
            else if (evt.PreviousState == GameState.Paused) {
                if (_backgroundPanel != null) {
                    _backgroundPanel.alpha = 0f;
                    _backgroundPanel.blocksRaycasts = false;
                    _backgroundPanel.interactable = false;
                }

                _menuService.Hide();
            }
        }

        public void OnResume() {
            PlayMenuSecondarySfx();
            _gameState.ChangeState(GameState.Gameplay);
        }

        public void OnSettings() {
            PlayMenuSecondarySfx();
            _menuService.Show(MenuScreen.Settings);
        }

        public void OnCredits() {
            PlayMenuSecondarySfx();
            _menuService.Show(MenuScreen.Credits);
        }

        public void OnQuit() {
            PlayMenuSecondarySfx();
            SceneManager.LoadScene("MainMenu");
        }
    }
}
