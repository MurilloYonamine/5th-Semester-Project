// autor: Murillo Gomes Yonamine
// data: 15/03/2026

using UnityEngine;

namespace FifthSemester.UI {
    public interface IMenuState {
        void EnterState();
        void ExitState();
        void OnReturn(GameObject caller);
    }
}
