// autor: Murillo Gomes Yonamine
// data: 15/03/2026

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace FifthSemester.UI {
    public class CreditsMenuState : MonoBehaviour, IMenuState {
        public void EnterState() {
            gameObject.SetActive(true);
        }
        public void ExitState() {
            gameObject.SetActive(false);
        }
        public void OnReturn(GameObject caller) {
            MenuManager.Instance.ChangeState(MenuManager.Instance.MainMenuState);
            EventSystem.current.SetSelectedGameObject(caller);
        }
        public override string ToString() {
            return "Credits Menu State";
        }
    }
}
