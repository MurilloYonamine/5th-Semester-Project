using System;

namespace FifthSemester.Core.Services
{
    public interface IPauseService 
    {
        void PauseGame();
        void ResumeGame();
        void TogglePause();

        bool IsPaused { get; }
    }
}