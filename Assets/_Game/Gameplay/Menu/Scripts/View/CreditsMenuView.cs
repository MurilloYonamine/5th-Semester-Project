using FifthSemester.Core.Enums;
using FifthSemester.Core.Services;
using FifthSemester.Core.States;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;
using UnityEngine.UI;

namespace FifthSemester.Gameplay.Menu {
    public class CreditsMenuView : MonoBehaviour {
        [Header("Focus")]
        [SerializeField] private GameObject _focusFirstElement;

        [Header("Buttons")]
        [SerializeField] private Button _backButton;

        private IMenuService _menuService;

        private void Start() {
            _menuService = ServiceLocator.Get<IMenuService>();

            _menuService.Register(MenuScreen.Credits, gameObject);
            _menuService.Show(MenuScreen.Credits);

            EventSystem.current.SetSelectedGameObject(_focusFirstElement);

            _backButton.onClick.AddListener(OnBack);
        }

        private void OnEnable() {
            EventSystem.current.SetSelectedGameObject(null);

            if (_focusFirstElement != null) {
                EventSystem.current.SetSelectedGameObject(_focusFirstElement);
            }

            InputSystem.onAnyButtonPress.Call(OnAnyInput);
        }

        private void OnDestroy() {
            _menuService?.Unregister(MenuScreen.Credits);
        }
        public void OnBack() {
            _menuService.Show(MenuScreen.MainMenu);
        }

        private void OnAnyInput(InputControl control) {
            if (control.device is Gamepad && EventSystem.current.currentSelectedGameObject == null) {
                EventSystem.current.SetSelectedGameObject(_focusFirstElement);
            }
        }
    }
}