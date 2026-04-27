using FifthSemester.Core.Enums;
using FifthSemester.Core.Services;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace FifthSemester.Gameplay.Menu {
    public class SettingsMenuView : MonoBehaviour {
        private IMenuService _menuService;

        [SerializeField] private GameObject _focusFirstElement;

        [Header("Buttons")]
        [SerializeField] private Button _audioSettingsButton;
        [SerializeField] private Button _graphicsSettingsButton;
        [SerializeField] private Button _gameplaySettingsButton;
        [SerializeField] private Button _screenSettingsButton;
        [SerializeField] private Button _backButton;


        private void Start() {
            _menuService = ServiceLocator.Get<IMenuService>();

            _menuService.Register(MenuScreen.Settings, gameObject);

            _audioSettingsButton.onClick.AddListener(OpenAudioSettings);
            _graphicsSettingsButton.onClick.AddListener(OpenGraphicsSettings);
            _gameplaySettingsButton.onClick.AddListener(OpenGameplaySettings);
            _screenSettingsButton.onClick.AddListener(OpenScreenSettings);
            _backButton.onClick.AddListener(OnBack);
        }

        private void OnDestroy() {
            _menuService?.Unregister(MenuScreen.Settings);
        }

        private void OnEnable() {
            if (_focusFirstElement != null) {
                EventSystem.current.SetSelectedGameObject(_focusFirstElement);
            }
        }

        public void OpenAudioSettings() => _menuService.Show(MenuScreen.Settings_Audio);
        public void OpenGraphicsSettings() => _menuService.Show(MenuScreen.Settings_Graphics);
        public void OpenGameplaySettings() => _menuService.Show(MenuScreen.Settings_Gameplay);
        public void OpenScreenSettings() => _menuService.Show(MenuScreen.Settings_Screen);

        public void OnBack() {
            _menuService.Show(MenuScreen.MainMenu);
        }
    }
}