using System;
using UnityEngine;

namespace FifthSemester.Core.Services
{
    public interface IDialogueService<TDialogue> where TDialogue : ScriptableObject
    {
        bool IsDialogueActive { get; }

        void StartDialogue(TDialogue dialogue);
        void DisplayNextLine();
        void EndDialogue();
    }
}