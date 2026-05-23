using FifthSemester.Core.Enums;
using FifthSemester.Core.Services;
using FifthSemester.Core.States;
using FifthSemester.Core.Events;
using UnityEngine;
using UnityEngine.UI;

namespace FifthSemester.Gameplay.Menu {
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
            _loadButton.onClick.AddListener(OnLoad);
            _settingsButton.onClick.AddListener(OnSettings);
            _creditsButton.onClick.AddListener(OnCredits);
            _quitButton.onClick.AddListener(OnQuit);
            
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
            _gameState.ChangeState(GameState.Gameplay);
        }

        public void OnLoad() {
            _menuService.Show(MenuScreen.LoadGame);
        }
        public void OnSettings() {
            _menuService.Show(MenuScreen.Settings);
        }

        public void OnCredits() {
            _menuService.Show(MenuScreen.Credits);
        }

        public void OnQuit() {
        }
    }
}
