using System;
using FifthSemester.Core.Events;
using FifthSemester.Core.States;

namespace FifthSemester.Core.Services
{
    public interface IPauseService 
    {
        GameState CurrentState { get; set; }

        void PauseGame();
        void ResumeGame();
        void TogglePause();

        bool IsPaused { get; }

        void OnGameStateChanged(GameStateChangedEvent evt);
    }
}