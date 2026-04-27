using FifthSemester.Core.Enums;
using FifthSemester.Core.Services;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

namespace FifthSemester.Gameplay.Menu {
    public abstract class MenuViewBase : MonoBehaviour {
        protected IMenuService _menuService;
        [SerializeField] protected GameObject _focusFirstElement;
        protected abstract MenuScreen MenuScreenType { get; }

        protected virtual void Start() {
            _menuService = ServiceLocator.Get<IMenuService>();
            _menuService.Register(MenuScreenType, gameObject);
        }

        protected virtual void OnEnable() {
            EventSystem.current.SetSelectedGameObject(null);
            if (_focusFirstElement != null) {
                EventSystem.current.SetSelectedGameObject(_focusFirstElement);
            }
            InputSystem.onAnyButtonPress.Call(OnAnyInput);
        }

        protected virtual void OnDestroy() {
            _menuService?.Unregister(MenuScreenType);
        }

        protected virtual void OnAnyInput(InputControl control) {
            if (control.device is Gamepad && EventSystem.current.currentSelectedGameObject == null) {
                EventSystem.current.SetSelectedGameObject(_focusFirstElement);
            }
        }
    }
}