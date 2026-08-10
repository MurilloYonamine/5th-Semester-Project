using FifthSemester.Core.Enums;
using FifthSemester.Core.Services;
using FifthSemester.Core.States;
using FifthSemester.Gameplay.Save;
<<<<<<< HEAD
using FifthSemester.Gameplay.Inventory;
=======
>>>>>>> origin/main
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
<<<<<<< HEAD

        private IGameStateService _gameState;
        private const string DEFAULT_SLOT = "default";
=======
        [SerializeField] private LoadGameView _loadGameView;
        [SerializeField] private CheckpointSO _initialCheckpoint;

        private IGameStateService _gameState;
>>>>>>> origin/main

        protected override MenuScreen MenuScreenType => MenuScreen.MainMenu;

        protected override void Start() {
            _gameState = ServiceLocator.Get<IGameStateService>();
            base.Start();

            _menuService.Show(MenuScreen.MainMenu);

<<<<<<< HEAD
            // Garante que o cursor do mouse esteja visível e destravado no Menu Principal
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;

=======
>>>>>>> origin/main
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
<<<<<<< HEAD

            // Garante que o cursor esteja liberado sempre que o painel for habilitado
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }

        public void OnPlay() {
            PlayMenuPrimarySfx();
            _menuService.Hide();
=======
        }

        public void OnPlay() {
>>>>>>> origin/main
            _gameState.ChangeState(GameState.Gameplay);
            SceneManager.LoadScene("Game");
        }

        public void OnNewGame() {
            var saveService = ServiceLocator.Get<ISaveService>();
<<<<<<< HEAD
            if (saveService != null) {
                saveService.DeleteSlot(DEFAULT_SLOT);
            }

            var inventoryService = ServiceLocator.Get<IInventoryService<Item>>();
            inventoryService?.Clear();

            SaveLoader.ClearPendingSave();
            PlayMenuPrimarySfx();

            var transitioner = GetComponent<GameTransitioner>();
            if (transitioner != null) {
                _menuService.Hide();
                _gameState.ChangeState(GameState.Cutscene);
                transitioner.StartGameSequence();
            }
            else {
                _menuService.Hide();
                _gameState.ChangeState(GameState.Gameplay);
                SceneManager.LoadScene("Game");
            }
        }
        public void OnContinue() {
            ISaveService saveService = ServiceLocator.Get<ISaveService>();
            if (saveService == null || !saveService.SlotExists(DEFAULT_SLOT)) {
                OnNewGame();
                return;
            }

            SaveData saveData = saveService.LoadFromSlot(DEFAULT_SLOT);
            if (saveData == null) {
                OnNewGame();
                return;
            }

            PlayMenuPrimarySfx();
            SaveLoader.SetPendingSave(saveData);
            _menuService.Hide();
            _gameState.ChangeState(GameState.Gameplay);
            
            string sceneToLoad = string.IsNullOrEmpty(saveData.SceneName) ? "Game" : saveData.SceneName;
            SceneManager.LoadScene(sceneToLoad);
        }
=======
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
>>>>>>> origin/main

        public void UpdateContinueVisibility() {
            if (_continueButtonContainer == null) return;

            ISaveService saveService = ServiceLocator.Get<ISaveService>();
<<<<<<< HEAD
            _continueButtonContainer.SetActive(saveService != null && saveService.SlotExists(DEFAULT_SLOT));
        }

        public void OnSettings() {
            PlayMenuSecondarySfx();
=======
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
>>>>>>> origin/main
            _menuService.Show(MenuScreen.Settings);
        }

        public void OnCredits() {
<<<<<<< HEAD
            PlayMenuSecondarySfx();
            _menuService.Show(MenuScreen.Credits);
        }
        public void OnQuit() {
            PlayMenuSecondarySfx();
=======
            _menuService.Show(MenuScreen.Credits);
        }
        public void OnQuit() {
>>>>>>> origin/main
            Application.Quit();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }
    }
}
