using FifthSemester.Core.Enums;
using FifthSemester.Core.Services;
using FifthSemester.Core.States;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

namespace FifthSemester.Gameplay.Menu {
    public class PauseMenuView : MonoBehaviour {
        private IGameStateService _gameState;
        private IMenuService _menuService;

        [Header("Focus")]
        [SerializeField] private GameObject _focusFirstElement;

        private void Start() {
            _gameState = ServiceLocator.Get<IGameStateService>();
            _menuService = ServiceLocator.Get<IMenuService>();
            
            _menuService.Register(MenuScreen.PauseMenu, gameObject);
        }
        private void OnEnable() {
            EventSystem.current.SetSelectedGameObject(null);

            if (_focusFirstElement != null) {
                EventSystem.current.SetSelectedGameObject(_focusFirstElement);
            }

            InputSystem.onAnyButtonPress.Call(OnAnyInput);
        }
        private void OnDestroy() {
            _menuService?.Unregister(MenuScreen.PauseMenu);
        }

        public void OnResume() {
            _gameState.ChangeState(GameState.Gameplay);
        }

        public void OnQuit() {
            _gameState.ChangeState(GameState.MainMenu);
        }

        private void OnAnyInput(InputControl control) {
            if (control.device is Gamepad && EventSystem.current.currentSelectedGameObject == null) {
                EventSystem.current.SetSelectedGameObject(_focusFirstElement);
            }
        }
    }
}