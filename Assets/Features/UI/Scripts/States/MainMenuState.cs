// autor: Murillo Gomes Yonamine
// data: 15/03/2026

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace FifthSemester.UI {
    public class MainMenuState : MonoBehaviour, IMenuState {
        public MenuManager MenuManager { get; private set; }

        public void EnterState(MenuManager menuManager) {
            MenuManager = menuManager;
            gameObject.SetActive(true);
        }
        public void ExitState(MenuManager menuManager) {
            gameObject.SetActive(false);
        }
        public override string ToString() {
            return "Main Menu State";
        }
        public void OnReturn(GameObject caller) {
            MenuManager.ChangeState(MenuManager.MainMenuState);
            EventSystem.current.SetSelectedGameObject(caller);
        }
    }
}
