using FifthSemester.Core.Enums;
using FifthSemester.Core.Services;
using FifthSemester.Core.States;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace FifthSemester.Gameplay.Menu {
    public class MainMenuView : MonoBehaviour {
        [Header("Focus")]
        [SerializeField] private GameObject _focusFirstElement;

        [Header("Buttons")]
        [SerializeField] private Button _playButton;
        [SerializeField] private Button _settingsButton;
        [SerializeField] private Button _creditsButton;
        [SerializeField] private Button _quitButton;

        private IGameStateService _gameState;
        private IMenuService _menuService;

        private void Start() {
            _gameState = ServiceLocator.Get<IGameStateService>();
            _menuService = ServiceLocator.Get<IMenuService>();

            _menuService.Register(MenuScreen.MainMenu, gameObject);
            _menuService.Show(MenuScreen.MainMenu);

            EventSystem.current.SetSelectedGameObject(_focusFirstElement);

            _playButton.onClick.AddListener(OnPlay);
            _settingsButton.onClick.AddListener(OnSettings);
            _creditsButton.onClick.AddListener(OnCredits);
            _quitButton.onClick.AddListener(OnQuit);
        }

        private void OnEnable() {
            EventSystem.current.SetSelectedGameObject(null);

            if (_focusFirstElement != null) {
                EventSystem.current.SetSelectedGameObject(_focusFirstElement);
            }

            InputSystem.onAnyButtonPress.Call(OnAnyInput);
        }

        private void OnDestroy() {
            _menuService?.Unregister(MenuScreen.MainMenu);
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
        private void OnAnyInput(InputControl control) {
            if (control.device is Gamepad && EventSystem.current.currentSelectedGameObject == null) {
                EventSystem.current.SetSelectedGameObject(_focusFirstElement);
            }
        }
    }
}