// autor: Murillo Gomes Yonamine
// data: 15/03/2026

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace FifthSemester.UI {
    public class MenuManager : MonoBehaviour, IManagerUI {
        public static MenuManager Instance { get; private set; }

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


        private void Awake() {
            if (Instance != null) {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start() {
            MainMenuState.gameObject.SetActive(false);
            SettingsMenuState.gameObject.SetActive(false);
            CreditsMenuState.gameObject.SetActive(false);

            EventSystem.current.SetSelectedGameObject(_playButton.gameObject);

            ChangeState(MainMenuState);
        }
        public void ChangeState(IMenuState newState) {
            if (_currentMenuState == newState) return; 

            _currentMenuState?.ExitState();

            _currentMenuState = newState;

            _currentMenuState?.EnterState();
        }
        public void OnReturn(GameObject caller) {
            if (caller != null && caller.activeInHierarchy) {
                EventSystem.current.SetSelectedGameObject(caller);
            }
            ChangeState(MainMenuState);
        }
    }
}
