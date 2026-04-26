using System.Collections.Generic;
using FifthSemester.Core.Enums;
using FifthSemester.Core.Services;
using Sirenix.OdinInspector;
using UnityEngine;

namespace FifthSemester.Gameplay.Menu {
    public class MenuService : IMenuService {
        private readonly Dictionary<MenuScreen, GameObject> _menus = new Dictionary<MenuScreen, GameObject>();

        public void Register(MenuScreen screen, GameObject view) {
            if (!_menus.ContainsKey(screen)) {
                _menus.Add(screen, view);
                view.SetActive(false);
            }
        }

        public void Unregister(MenuScreen screen) {
            if (_menus.ContainsKey(screen)) {
                _menus.Remove(screen);
            }
        }

        public void Hide() {
            foreach (var menu in _menus.Values) {
                if (menu != null) {
                    menu.SetActive(false);
                }
            }
        }

        public void Show(MenuScreen screen) {
            Hide();
            if (_menus.TryGetValue(screen, out var view) && view != null) {
                view.SetActive(true);
            }
        }
    }
}