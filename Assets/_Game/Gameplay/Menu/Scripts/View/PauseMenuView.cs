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

        [Header("Buttons")]
        [SerializeField] private Button _resumeButton;
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
        }

        protected override void OnDestroy() {
            base.OnDestroy();
            _eventBus?.Unsubscribe<GameStateChangedEvent>(OnGameStateChanged);
        }

        private void OnGameStateChanged(GameStateChangedEvent evt) {
            if (evt.CurrentState == GameState.Paused) {
                _menuService.Show(MenuScreen.PauseMenu);

                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
            }
            else if (evt.PreviousState == GameState.Paused) {
                _menuService.Hide();

                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;

            }
        }

        public void OnResume() {
            _gameState.ChangeState(GameState.Gameplay);
        }

        public void OnSettings() {
            _menuService.Show(MenuScreen.Settings);
        }

        public void OnCredits() {
            _menuService.Show(MenuScreen.Credits);
        }

        public void OnQuit() {
            _gameState.ChangeState(GameState.MainMenu);
        }
    }
}
