using FifthSemester.Core.Enums;
using FifthSemester.Core.Services;
using FifthSemester.Core.States;

namespace FifthSemester.Gameplay.Menu {

    public class PauseMenuView : MenuViewBase {
        private IGameStateService _gameState;
        protected override MenuScreen MenuScreenType => MenuScreen.PauseMenu;

        protected override void Start() {
            _gameState = ServiceLocator.Get<IGameStateService>();
            base.Start();
        }

        public void OnResume() {
            _gameState.ChangeState(GameState.Gameplay);
        }

        public void OnQuit() {
            _gameState.ChangeState(GameState.MainMenu);
        }
    }
}