using System.Collections.Generic;
using FifthSemester.Core.Enums;
using FifthSemester.Core.Services;
using FifthSemester.Shared;
using UnityEngine;

namespace FifthSemester.Gameplay {
    public class MenuService : IMenuService {
        private readonly Dictionary<MenuScreen, CanvasGroup> _menus = new Dictionary<MenuScreen, CanvasGroup>();

        public void Register(MenuScreen screen, GameObject view) {
            if (!_menus.ContainsKey(screen)) {
                if (!view.TryGetComponent<CanvasGroup>(out var canvasGroup)) {
                    canvasGroup = view.AddComponent<CanvasGroup>();
                }
                _menus.Add(screen, canvasGroup);
                SetCanvasGroupState(canvasGroup, false);
            }
        }

        public void Unregister(MenuScreen screen) {
            if (_menus.ContainsKey(screen)) {
                _menus.Remove(screen);
            }
        }

        public GameObject GetView(MenuScreen screen) {
            _menus.TryGetValue(screen, out var canvasGroup);
            return canvasGroup != null ? canvasGroup.gameObject : null;
        }

        public void Hide() {
            foreach (var canvasGroup in _menus.Values) {
                if (canvasGroup != null) {
                    SetCanvasGroupState(canvasGroup, false);
                    if (canvasGroup.TryGetComponent<IMenuView>(out var menuView)) {
                        menuView.OnHide();
                    }
                }
            }
        }

        public void Show(MenuScreen screen) {
            Hide();
            if (_menus.TryGetValue(screen, out var canvasGroup) && canvasGroup != null) {
                SetCanvasGroupState(canvasGroup, true);
                if (canvasGroup.TryGetComponent<IMenuView>(out var menuView)) {
                    menuView.OnShow();
                }
            }
        }

        private void SetCanvasGroupState(CanvasGroup canvasGroup, bool isVisible) {
            canvasGroup.alpha = isVisible ? 1f : 0f;
            canvasGroup.interactable = isVisible;
            canvasGroup.blocksRaycasts = isVisible;
        }
    }
}
