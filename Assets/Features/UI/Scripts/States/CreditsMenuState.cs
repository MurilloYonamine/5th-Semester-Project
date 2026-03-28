// autor: Murillo Gomes Yonamine
// data: 15/03/2026

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace FifthSemester.UI {
    public class CreditsMenuState : MonoBehaviour, IMenuState {
        public MenuManager MenuManager { get; private set; }

        public void EnterState(MenuManager menuManager) {
            MenuManager = menuManager;
            gameObject.SetActive(true);
        }
        public void ExitState(MenuManager menuManager) {
            gameObject.SetActive(false);
        }
        public void OnReturn(GameObject caller) {
            MenuManager.ChangeState(MenuManager.MainMenuState);
            EventSystem.current.SetSelectedGameObject(caller);
        }
        public override string ToString() {
            return "Credits Menu State";
        }
    }
}
