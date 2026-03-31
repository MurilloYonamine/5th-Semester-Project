// autor: Murillo Gomes Yonamine
// data: 29/03/2026

using FifthSemester.Core.Events;
using FifthSemester.Core.Managers;
using FifthSemester.Core.States;
using UnityEngine;
using UnityEngine.EventSystems;

namespace FifthSemester.UI {
    public class PauseMenuManager : MonoBehaviour, IManagerUI {
        public static PauseMenuManager Instance { get; private set; }

        [Header("UI References")]
        [SerializeField] private GameObject pauseCanvas;

        [field: SerializeField] public IMenuState CurrentMenuState { get; private set; }
        [field: SerializeField] public MainMenuState MainMenuState { get; private set; }
        [field: SerializeField] public SettingsMenuState SettingsMenuState { get; private set; }
        [field: SerializeField] public CreditsMenuState CreditsMenuState { get; private set; }

        private void OnEnable() {
            GameStateManager.OnStateChanged += HandleGameStateChanged;
            InputEvents.Instance.OnOpenPause += TogglePause;
        }

        private void OnDisable() {
            GameStateManager.OnStateChanged -= HandleGameStateChanged;
            InputEvents.Instance.OnOpenPause -= TogglePause;
        }

        public void ChangeState(IMenuState newState) {
            if (CurrentMenuState == newState) return;

            CurrentMenuState?.ExitState();

            CurrentMenuState = newState;

            CurrentMenuState?.EnterState();
        }

        private void TogglePause() {
            if (GameStateManager.Instance.CurrentState == GameState.Pause) {
                if (!Equals(CurrentMenuState, MainMenuState)) {
                    ChangeState(MainMenuState);
                    return;
                }
                GameStateManager.Instance.ChangeState(GameState.Gameplay);
            }
            else if (GameStateManager.Instance.CurrentState == GameState.Gameplay) {
                ChangeState(MainMenuState);
                GameStateManager.Instance.ChangeState(GameState.Pause);
            }
        }

        private void HandleGameStateChanged(GameState newState) {
            if (newState == GameState.Pause) {
                pauseCanvas.SetActive(true);
                Time.timeScale = 0f;
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else if (newState == GameState.Gameplay) {
                pauseCanvas.SetActive(false);
                Time.timeScale = 1f;
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }

        public void Resume() {
            GameStateManager.Instance.ChangeState(GameState.Gameplay);
        }

        public void Pause() {
            GameStateManager.Instance.ChangeState(GameState.Pause);
        }

        public void QuitToMainMenu() {
            Time.timeScale = 1f;
            SceneLoaderManager.Instance.LoadNewLevel("MainMenu");
        }

        public void OnReturn(GameObject caller) {
            if (caller != null && caller.activeInHierarchy) {
                EventSystem.current.SetSelectedGameObject(caller);
            }
            ChangeState(MainMenuState);
        }
    }
}
