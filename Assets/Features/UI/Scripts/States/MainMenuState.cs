// autor: Murillo Gomes Yonamine
// data: 15/03/2026

using UnityEngine;
using UnityEngine.UI;

namespace FifthSemester.UI {
    public class MainMenuState : MonoBehaviour, IMenuState {
        public void EnterState(MenuManager menuManager) {
            gameObject.SetActive(true);
        }
        public void ExitState(MenuManager menuManager) {
            gameObject.SetActive(false);
        }
        public override string ToString() {
            return "Main Menu State";
        }
    }
}
