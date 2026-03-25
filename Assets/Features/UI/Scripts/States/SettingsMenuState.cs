using UnityEngine;
using UnityEngine.EventSystems;

namespace FifthSemester.UI {
    public class SettingsMenuState : MonoBehaviour, IMenuState {
        public MenuManager MenuManager { get; private set; }
        private ISettingsState _currentSubState;
        [Header("Settings Panels")]
        [SerializeField] private SettingsChoiceState _settingsChoiceState;
        [SerializeField] private AudioState _audioState;
        [SerializeField] private VideoState _videoState;
        [SerializeField] private ControllerState _controllerState;
        [SerializeField] private PostProcessingState _postProcessingState;

        public void EnterState(MenuManager menuManager) {
            gameObject.SetActive(true);
            MenuManager = menuManager;

            _currentSubState = _settingsChoiceState;
            ChangeState(_currentSubState);
        }

        public void ExitState(MenuManager menuManager) {
            gameObject.SetActive(false);
        }
        #region Button Pressed Methods
        public void OnAudioButtonPressed(GameObject caller) {
            if (caller != null)
                EventSystem.current.SetSelectedGameObject(caller);
            ChangeState(_audioState);
        }
        public void OnVideoButtonPressed(GameObject caller) {
            if (caller != null)
                EventSystem.current.SetSelectedGameObject(caller);
            ChangeState(_videoState);
        }
        public void OnControllerButtonPressed(GameObject caller) {
            if (caller != null)
                EventSystem.current.SetSelectedGameObject(caller);
            ChangeState(_controllerState);
        }
        public void OnPostProcessingButtonPressed(GameObject caller) {
            if (caller != null)
                EventSystem.current.SetSelectedGameObject(caller);
            ChangeState(_postProcessingState);
        }
        public void OnBackReturnPressed(GameObject caller) {
            if (caller != null)
                EventSystem.current.SetSelectedGameObject(caller);
            ChangeState(_settingsChoiceState);
        }
        #endregion

        public void ChangeState(ISettingsState newState) {
            Debug.Log($"Changing sub-menu state to: {newState.ToString()}");

            _currentSubState?.ExitState(this);

            _currentSubState = newState;

            _currentSubState?.EnterState(this);
        }

        public void OnReturn(GameObject caller) {
            MenuManager.ChangeState(MenuManager.MainMenuState);
            EventSystem.current.SetSelectedGameObject(caller);
            
            if (caller.TryGetComponent<ButtonHover>(out var hover))
                hover.OnSelect(null);
        }
        public override string ToString() {
            return "Settings Menu State";
        }
    }
}
