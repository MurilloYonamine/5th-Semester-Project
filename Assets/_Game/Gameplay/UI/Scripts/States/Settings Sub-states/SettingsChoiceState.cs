using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace FifthSemester.UI
{
    public class SettingsChoiceState : MonoBehaviour, ISettingsState {
        public void EnterState(SettingsMenuState settingsMenuState) {
            gameObject.SetActive(true);
        }

        public void ExitState(SettingsMenuState settingsMenuState) {
            gameObject.SetActive(false);
        }
    }
}
