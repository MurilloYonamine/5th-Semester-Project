using System.Collections;
using FifthSemester.Framework.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace FifthSemester.UI {
    public class SettingsMenuState : MonoBehaviour, IMenuState {
        private enum SettingsSubState {
            Choice,
            Audio,
            Video,
            Controller,
            PostProcessing
        }

        [SerializeField] private SettingsSubState _currentSettingsSubState;
        private ISettingsState _currentSubState;

        [Header("Settings Panels")]
        [SerializeField] private SettingsChoiceState _settingsChoiceState;
        [SerializeField] private AudioState _audioState;
        [SerializeField] private VideoState _videoState;
        [SerializeField] private ControllerState _controllerState;
        [SerializeField] private PostProcessingState _postProcessingState;


        public void EnterState(MenuManager menuManager) {
            gameObject.SetActive(true);
            ChangeState(_currentSettingsSubState);
        }
        public void ExitState(MenuManager menuManager) {
            gameObject.SetActive(false);
        }
        #region Button Pressed Methods
        public void OnAudioButtonPressed() {
            ChangeState(SettingsSubState.Audio);
        }
        public void OnVideoButtonPressed() {
            ChangeState(SettingsSubState.Video);
        }
        public void OnControllerButtonPressed() {
            ChangeState(SettingsSubState.Controller);
        }
        public void OnPostProcessingButtonPressed() {
            ChangeState(SettingsSubState.PostProcessing);
        }
        public void OnBackReturnPressed() {
            ChangeState(SettingsSubState.Choice);
        }
        #endregion

        public void ChangeState(ISettingsState newState) {
            Debug.Log($"Changing sub-menu state to: {newState.ToString()}");

            _currentSubState?.ExitState(this);

            _currentSubState = newState;

            _currentSubState?.EnterState(this);
        }
        private void ChangeState(SettingsSubState subState) {
            _currentSettingsSubState = subState;
            switch (subState) {
                case SettingsSubState.Choice:
                    ChangeState(_settingsChoiceState);
                    break;
                case SettingsSubState.Audio:
                    ChangeState(_audioState);
                    break;
                case SettingsSubState.Video:
                    ChangeState(_videoState);
                    break;
                case SettingsSubState.Controller:
                    ChangeState(_controllerState);
                    break;
                case SettingsSubState.PostProcessing:
                    ChangeState(_postProcessingState);
                    break;
            }
        }

        public override string ToString() {
            return "Settings Menu State";
        }
    }
}
