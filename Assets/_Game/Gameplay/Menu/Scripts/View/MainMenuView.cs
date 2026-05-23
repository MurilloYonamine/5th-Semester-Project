using FifthSemester.Core.Enums;
using FifthSemester.Core.Services;
using FifthSemester.Core.States;
using FifthSemester.Gameplay.Save;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace FifthSemester.Gameplay.Menu {

    public class MainMenuView : MenuViewBase {

        [Header("Buttons")]
        [SerializeField] private Button _newGameButton;
        [SerializeField] private Button _continueButton;
        [SerializeField] private Button _settingsButton;
        [SerializeField] private Button _creditsButton;
        [SerializeField] private Button _quitButton;

        [Header("Continue UI")]
        [SerializeField] private GameObject _continueButtonContainer;
        [SerializeField] private LoadGameView _loadGameView;
        [SerializeField] private CheckpointSO _initialCheckpoint;

        private IGameStateService _gameState;

        protected override MenuScreen MenuScreenType => MenuScreen.MainMenu;

        protected override void Start() {
            _gameState = ServiceLocator.Get<IGameStateService>();
            base.Start();

            _menuService.Show(MenuScreen.MainMenu);

            _newGameButton.onClick.AddListener(OnNewGame);
            _continueButton.onClick.AddListener(OnContinue);
            _settingsButton.onClick.AddListener(OnSettings);
            _creditsButton.onClick.AddListener(OnCredits);
            _quitButton.onClick.AddListener(OnQuit);

            UpdateContinueVisibility();
        }

        protected override void OnEnable() {
            base.OnEnable();
            UpdateContinueVisibility();
        }

        public void OnPlay() {
            _gameState.ChangeState(GameState.Gameplay);
            SceneManager.LoadScene("Game");
        }

        public void OnNewGame() {
            var saveService = ServiceLocator.Get<ISaveService>();
            if (saveService == null || _initialCheckpoint == null) {
                OnPlay();
                return;
            }

            string chosen = null;
            for (int i = 0; i < 3; i++) {
                string id = $"slot_{i}";
                if (!saveService.SlotExists(id)) {
                    chosen = id;
                    break;
                }
            }
            if (chosen == null) chosen = "slot_0";

            var data = new SaveData() {
                LastCheckpointId = _initialCheckpoint.Id
            };

            saveService.SaveToSlot(chosen, data);

            _gameState.ChangeState(GameState.Gameplay);
            SceneManager.LoadScene("Game");
        }

        public void OnContinue() {
            if (_loadGameView != null) {
                _menuService.Show(MenuScreen.LoadGame); 
            }
            else {
                OnPlay();
            }
        }

        public void UpdateContinueVisibility() {
            if (_continueButtonContainer == null) return;

            ISaveService saveService = ServiceLocator.Get<ISaveService>();
            _continueButtonContainer.SetActive(HasSavedGame(saveService));
        }

        private bool HasSavedGame(ISaveService saveService) {
            if (saveService == null) return false;

            for (int i = 0; i < 3; i++) {
                if (saveService.SlotExists($"slot_{i}")) {
                    return true;
                }
            }

            return false;
        }

        public void OnSettings() {
            _menuService.Show(MenuScreen.Settings);
        }

        public void OnCredits() {
            _menuService.Show(MenuScreen.Credits);
        }
        public void OnQuit() {
            Application.Quit();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }
    }
}
