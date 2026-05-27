using FifthSemester.Core.Enums;
using UnityEngine;
using UnityEngine.UI;

namespace FifthSemester.Gameplay.Menu {

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
            _menuService.Show(MenuScreen.MainMenu);
        }
    }
}