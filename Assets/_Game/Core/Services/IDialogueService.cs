using FifthSemester.Core.Enums;
using FifthSemester.Core.Events;
using FifthSemester.Core.States;
using UnityEngine.Playables;

namespace FifthSemester.Core.Services {
    public interface IDialogueService<TDialogue> {
        DialogueMode CurrentMode { get; }
        GameState CurrentState { get; set; }
        bool IsDialogueActive { get; }

        void StartDialogue(TDialogue dialogue, PlayableDirector director = null, string sourceId = null, DialogueMode mode = DialogueMode.Normal);
        void DisplayNextLine();
        void EndDialogue();

        void OnGameStateChanged(GameStateChangedEvent evt);
        void TimelineShowLine();
    }
}
