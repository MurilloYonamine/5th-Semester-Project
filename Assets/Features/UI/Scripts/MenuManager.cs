// autor: Murillo Gomes Yonamine
// data: 15/03/2026

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace FifthSemester.UI {
    public class MenuManager : MonoBehaviour {
        [SerializeField] private IMenuState _currentMenuState;
        [SerializeField] private IMenuState _previousMenuState;

        [field: SerializeField] public MainMenuState MainMenuState { get; private set; }
        [field: SerializeField] public SettingsMenuState SettingsMenuState { get; private set; }
        [field: SerializeField] public CreditsMenuState CreditsMenuState { get; private set; }

        [Header("Buttons")]
        [SerializeField] private Button _playButton;
        [SerializeField] private Button _settingsButton;
        [SerializeField] private Button _creditsButton;
        [SerializeField] private Button _exitButton;

        [SerializeField] private AudioClip _returnSFX;

        private void Start() {
            MainMenuState.gameObject.SetActive(false);
            SettingsMenuState.gameObject.SetActive(false);
            CreditsMenuState.gameObject.SetActive(false);

            _currentMenuState = MainMenuState;

            ChangeState(_currentMenuState);
        }
        public void ChangeState(IMenuState newState) {
            Debug.Log($"Changing menu state to: {newState.ToString()}");

            _currentMenuState?.ExitState(this);

            _currentMenuState = newState;

            _currentMenuState?.EnterState(this);
        }

        #region Button Callbacks
        public void OpenMainMenu(GameObject caller) {
            if (caller != null)
                EventSystem.current.SetSelectedGameObject(caller);
            ChangeState(MainMenuState);
        }
        public void OpenSettings(GameObject caller) {
            if (caller != null)
                EventSystem.current.SetSelectedGameObject(caller);
            ChangeState(SettingsMenuState);
        }
        public void OpenCredits(GameObject caller) {
            if (caller != null)
                EventSystem.current.SetSelectedGameObject(caller);
            ChangeState(CreditsMenuState);
        }
        public void Exit() {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
                        Application.Quit();
#endif
        }
        #endregion
        public void OnReturn(GameObject caller) {
            ChangeState(MainMenuState);
            EventSystem.current.SetSelectedGameObject(caller);
        }
    }
}
