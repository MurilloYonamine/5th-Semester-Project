using FifthSemester.Core.Enums;
using FifthSemester.Core.Services;
using UnityEngine;

namespace FifthSemester.Gameplay.Menu {
    public class GraphicsSettingsView : MonoBehaviour {
        private IMenuService _menuService;
        private ISettingsService _settingsService;

        private void Start() {
            _menuService = ServiceLocator.Get<IMenuService>();
            _settingsService = ServiceLocator.Get<ISettingsService>();

            _menuService.Register(MenuScreen.Settings_Graphics, gameObject);
        }

        public void OnBack() {
            _menuService.Show(MenuScreen.Settings);
        }
    }
}