// autor: Murillo Gomes Yonamine
// data: 29/03/2026

using FifthSemester.Core.Events;
using UnityEngine;
using UnityEngine.EventSystems;
using FifthSemester.Core.Services;

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
            var eventBus = ServiceLocator.Get<IEventBus>();
            eventBus?.Subscribe<PauseToggleRequestedEvent>(TogglePause);
        }

        private void OnDisable() {
            var eventBus = ServiceLocator.Get<IEventBus>();
            eventBus?.Unsubscribe<PauseToggleRequestedEvent>(TogglePause);
        }

        public void ChangeState(IMenuState newState) {
            if (CurrentMenuState == newState) return;

            CurrentMenuState?.ExitState();

            CurrentMenuState = newState;

            CurrentMenuState?.EnterState();
        }

        private void TogglePause(PauseToggleRequestedEvent evt) {
            ChangeState(MainMenuState);
        }


        public void Resume() {
        }

        public void Pause() {
        }

        public void QuitToMainMenu() {
            Time.timeScale = 1f;
        }

        public void OnReturn(GameObject caller) {
            if (caller != null && caller.activeInHierarchy) {
                EventSystem.current.SetSelectedGameObject(caller);
            }
            ChangeState(MainMenuState);
        }
    }
}
