using FifthSemester.Core.Enums;
using FifthSemester.Core.Services;
using FifthSemester.Core.States;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace FifthSemester.Gameplay.Menu {

    public class MainMenuView : MenuViewBase {

        [Header("Buttons")]
        [SerializeField] private Button _playButton;
        [SerializeField] private Button _settingsButton;
        [SerializeField] private Button _creditsButton;
        [SerializeField] private Button _quitButton;

        private IGameStateService _gameState;

        protected override MenuScreen MenuScreenType => MenuScreen.MainMenu;

        protected override void Start() {
            _gameState = ServiceLocator.Get<IGameStateService>();
            base.Start();
            _menuService.Show(MenuScreen.MainMenu);
            _playButton.onClick.AddListener(OnPlay);
            _settingsButton.onClick.AddListener(OnSettings);
            _creditsButton.onClick.AddListener(OnCredits);
            _quitButton.onClick.AddListener(OnQuit);
        }

        public void OnPlay() {
            _gameState.ChangeState(GameState.Gameplay);
            SceneManager.LoadScene("Gym");
        }

        public void OnSettings() {
            _menuService.Show(MenuScreen.Settings);
        }

        public void OnCredits() {
            _menuService.Show(MenuScreen.Credits);
        }
        public void OnQuit() {
            Application.Quit();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }
        // OnAnyInput herdado da base
    }
}