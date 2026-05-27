using FifthSemester.Core.Enums;
using UnityEngine;
using UnityEngine.UI;

namespace FifthSemester.Gameplay.Menu {

    public class SettingsMenuView : MenuViewBase {

        [Header("Buttons")]
        [SerializeField] private Button _audioSettingsButton;
        [SerializeField] private Button _graphicsSettingsButton;
        [SerializeField] private Button _gameplaySettingsButton;
        [SerializeField] private Button _screenSettingsButton;
        [SerializeField] private Button _backButton;

        protected override MenuScreen MenuScreenType => MenuScreen.Settings;

        protected override void Start() {
            base.Start();
            _audioSettingsButton.onClick.AddListener(OpenAudioSettings);
            if(_graphicsSettingsButton != null) _graphicsSettingsButton.onClick.AddListener(OpenGraphicsSettings);
            _gameplaySettingsButton.onClick.AddListener(OpenGameplaySettings);
            _screenSettingsButton.onClick.AddListener(OpenScreenSettings);
            _backButton.onClick.AddListener(OnBack);
        }

        public void OpenAudioSettings() {
            PlayMenuSecondarySfx();
            _menuService.Show(MenuScreen.Settings_Audio);
        }

        public void OpenGraphicsSettings() {
            PlayMenuSecondarySfx();
            _menuService.Show(MenuScreen.Settings_Graphics);
        }

        public void OpenGameplaySettings() {
            PlayMenuSecondarySfx();
            _menuService.Show(MenuScreen.Settings_Gameplay);
        }

        public void OpenScreenSettings() {
            PlayMenuSecondarySfx();
            _menuService.Show(MenuScreen.Settings_Screen);
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
