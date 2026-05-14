using FifthSemester.Core.Events;
using FifthSemester.Core.States;
using UnityEngine.Playables;

namespace FifthSemester.Core.Services {
    public interface IDialogueService<TDialogue> {
        GameState CurrentState { get; set; }
        bool IsDialogueActive { get; }

        void StartDialogue(TDialogue dialogue, PlayableDirector director = null);
        void DisplayNextLine();
        void EndDialogue();

        void OnGameStateChanged(GameStateChangedEvent evt);
        void TimelineShowLineAndPause();

    }
}
