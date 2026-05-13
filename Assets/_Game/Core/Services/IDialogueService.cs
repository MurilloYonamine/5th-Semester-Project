using FifthSemester.Core.Events;
using FifthSemester.Core.States;

namespace FifthSemester.Core.Services {
    public interface IDialogueService<TDialogue> {
        GameState CurrentState { get; set; }
        bool IsDialogueActive { get; }

        void StartDialogue(TDialogue dialogue);
        void DisplayNextLine();
        void EndDialogue();

        void OnGameStateChanged(GameStateChangedEvent evt);
    }
}
