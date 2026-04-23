using System;
using UnityEngine;

namespace FifthSemester.Core.Services
{
    public interface IDialogueService<TDialogue> where TDialogue : ScriptableObject
    {
        void StartDialogue(TDialogue dialogue);
        void DisplayNextLine();
        void EndDialogue();

        bool IsDialogueActive { get; }

        event Action OnDialogueStarted;
        event Action OnDialogueEnded;
    }
}