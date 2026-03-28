// autor: Murillo Gomes Yonamine
// data: 15/03/2026

using UnityEngine;

namespace FifthSemester.UI {
    public interface IMenuState {
        public MenuManager MenuManager { get; }
        void EnterState(MenuManager menuManager);
        void ExitState(MenuManager menuManager);
        void OnReturn(GameObject caller);
    }
}
