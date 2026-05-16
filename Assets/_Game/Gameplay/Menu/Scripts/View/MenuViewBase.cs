using FifthSemester.Core.Enums;
using FifthSemester.Core.Services;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

namespace FifthSemester.Gameplay.Menu {
    [RequireComponent(typeof(CanvasGroup))]
    public abstract class MenuViewBase : MonoBehaviour, IMenuView {
        protected IMenuService _menuService;
        [SerializeField] protected GameObject _focusFirstElement;
        protected abstract MenuScreen MenuScreenType { get; }

        protected CanvasGroup _canvasGroup;

        protected virtual void Awake() {
            _canvasGroup = GetComponent<CanvasGroup>();
        }

        protected virtual void Start() {
            _menuService = ServiceLocator.Get<IMenuService>();
            _menuService.Register(MenuScreenType, gameObject);
        }

        protected virtual void OnEnable() {
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

        public virtual void OnShow() {
            _canvasGroup.alpha = 1f;
            _canvasGroup.interactable = true;
            _canvasGroup.blocksRaycasts = true;

            EventSystem.current.SetSelectedGameObject(null);
            if (_focusFirstElement != null) {
                EventSystem.current.SetSelectedGameObject(_focusFirstElement);
            }
        }

        public virtual void OnHide() {
            _canvasGroup.alpha = 0f;
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;
        }
    }
}
