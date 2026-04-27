using FifthSemester.Core.Events;
using FifthSemester.Core.States;

namespace FifthSemester.Core.Input {
    public interface IInputService {
        GameState CurrentGameState { get; set; }
        void Enable();
        void Disable();

        public void OnGameStateChanged(GameStateChangedEvent evt);
    }
}