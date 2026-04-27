using FifthSemester.Core.Events;
using FifthSemester.Core.States;
using UnityEngine;

namespace FifthSemester.Core.Services {
    public interface IDialogueService<TDialogue> where TDialogue : ScriptableObject {
        GameState CurrentState { get; set; }
        bool IsDialogueActive { get; }

        void StartDialogue(TDialogue dialogue);
        void DisplayNextLine();
        void EndDialogue();

        void OnGameStateChanged(GameStateChangedEvent evt);
    }
}