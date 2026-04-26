using FifthSemester.Core.Enums;
using FifthSemester.Core.Services;
using FifthSemester.Core.States;
using UnityEngine;

namespace FifthSemester.Gameplay.Menu {
    public class PauseMenuView : MonoBehaviour {
        private IGameStateService _gameState;
        private IMenuService _menuService;

        private void Start() {
            _gameState = ServiceLocator.Get<IGameStateService>();
            _menuService = ServiceLocator.Get<IMenuService>();
            
            _menuService.Register(MenuScreen.PauseMenu, gameObject);
        }
        private void OnDestroy() {
            _menuService?.Unregister(MenuScreen.PauseMenu);
        }

        public void OnResume() {
            _gameState.ChangeState(GameState.Gameplay);
        }

        public void OnQuit() {
            _gameState.ChangeState(GameState.MainMenu);
        }
    }
}