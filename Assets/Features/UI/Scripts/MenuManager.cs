// autor: Murillo Gomes Yonamine
// data: 15/03/2026

using FifthSemester.Framework.UI;
using FifthSemester.Shared.AudioSystem;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace FifthSemester.UI {
    public class MenuManager : MonoBehaviour {
        private enum MenuType {
            Main,
            Settings,
            Credits
        }
        [SerializeField] private MenuType _currentMenu;

        [SerializeField] private IMenuState _currentMenuState;
        [SerializeField] private IMenuState _previousMenuState;

        [SerializeField] private MainMenuState _mainMenuState;
        [SerializeField] private SettingsMenuState _settingsMenuState;
        [SerializeField] private CreditsMenuState _creditsMenuState;

        [Header("Buttons")]
        [SerializeField] private Button _playButton;
        [SerializeField] private Button _settingsButton;
        [SerializeField] private Button _creditsButton;
        [SerializeField] private Button _exitButton;

        [SerializeField] private AudioClip _returnSFX;

        [SerializeField] private InputActionReference _return;
        private void Start() {
            _mainMenuState.gameObject.SetActive(false);
            _settingsMenuState.gameObject.SetActive(false);
            _creditsMenuState.gameObject.SetActive(false);

            ChangeState(_currentMenu);
        }
        private void OnEnable() {
            _return.action.Enable();
            _return.action.performed += OnReturn;
        }
        private void OnDisable() {
            _return.action.Disable();
            _return.action.performed -= OnReturn;
        }

        public void ChangeState(IMenuState newState) {
            Debug.Log($"Changing menu state to: {newState.ToString()}");

            _currentMenuState?.ExitState(this);

            _currentMenuState = newState;

            _currentMenuState?.EnterState(this);
        }
        private void ChangeState(MenuType menuType) {
            switch (menuType) {
                case MenuType.Main:
                    ChangeState(_mainMenuState);
                    break;
                case MenuType.Settings:
                    ChangeState(_settingsMenuState);
                    break;
                case MenuType.Credits:
                    ChangeState(_creditsMenuState);
                    break;
                default:
                    Debug.LogWarning("Unknown menu type: " + menuType);
                    break;
            }
        }

        #region Button Callbacks
        public void OpenMainMenu() {
            ChangeState(_mainMenuState);
        }
        public void OpenSettings() {
            ChangeState(_settingsMenuState);
        }
        public void OpenCredits() {
            ChangeState(_creditsMenuState);
        }
        public void Exit() {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
                        Application.Quit();
#endif
        }
        public void Return() {
            OnReturn(new InputAction.CallbackContext());
        }
        #endregion
        public void OnReturn(InputAction.CallbackContext context) {
            ChangeState(_mainMenuState);
        }
    }
}
