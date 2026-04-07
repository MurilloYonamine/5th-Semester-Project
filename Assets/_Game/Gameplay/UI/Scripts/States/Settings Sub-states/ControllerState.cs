using UnityEngine;

namespace FifthSemester.UI
{
    public class ControllerState : MonoBehaviour, ISettingsState {
        public void EnterState(SettingsMenuState settingsMenuState) {
            gameObject.SetActive(true);
        }

        public void ExitState(SettingsMenuState settingsMenuState) {
            gameObject.SetActive(false);
        }
    }
}
