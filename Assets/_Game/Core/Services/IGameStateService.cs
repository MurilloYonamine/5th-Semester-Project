using FifthSemester.Core.States;

namespace FifthSemester.Core.Services {
    public interface IGameStateService {
        GameState CurrentState { get; set; }
        void ChangeState(GameState newState);
    }
}