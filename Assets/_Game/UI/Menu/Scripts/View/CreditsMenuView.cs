using FifthSemester.Core.Enums;
using UnityEngine;
using UnityEngine.UI;

namespace FifthSemester.UI {

    public class CreditsMenuView : MenuViewBase {

        [Header("Buttons")]
        [SerializeField] private Button _backButton;

        protected override MenuScreen MenuScreenType => MenuScreen.Credits;

        protected override void Start() {
            base.Start();
            _backButton.onClick.AddListener(OnBack);
        }
        public void OnBack() {
            PlayMenuBackSfx();
            var pauseMenu = _menuService.GetView(MenuScreen.PauseMenu);
            if (pauseMenu != null && pauseMenu.activeSelf) {
                _menuService.Show(MenuScreen.PauseMenu);
            } else {
                _menuService.Show(MenuScreen.MainMenu);
            }
        }
    }
}