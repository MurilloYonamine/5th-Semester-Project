using FifthSemester.Core.Events;
using FifthSemester.Core.States;

namespace FifthSemester.Core.Input {
    public interface IInputService {
        GameState CurrentGameState { get; set; }

        public bool LastPauseWasGamepad { get; }
        public bool LastLookWasGamepad { get; }

        void Enable();
        void Disable();

        public void OnGameStateChanged(GameStateChangedEvent evt);
    }
}
