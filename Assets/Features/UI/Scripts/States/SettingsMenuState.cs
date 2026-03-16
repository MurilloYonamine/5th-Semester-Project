using System.Collections;
using FifthSemester.Framework.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace FifthSemester.UI {
    public class SettingsMenuState : MonoBehaviour, IMenuState {
        [SerializeField] private Selectable _firstSelectable;

        public void EnterState(MenuManager menuManager) {
            gameObject.SetActive(true);
            EventSystem.current.sendNavigationEvents = true;

            StartCoroutine(SelectNextFrame());
        }
        private IEnumerator SelectNextFrame() {
            yield return null;

            if (_firstSelectable != null) {
                EventSystem.current.SetSelectedGameObject(_firstSelectable.gameObject);
            }
        }
        public void ExitState(MenuManager menuManager) {
            gameObject.SetActive(false);
        }
        public override string ToString() {
            return "Settings Menu State";
        }
    }
}
