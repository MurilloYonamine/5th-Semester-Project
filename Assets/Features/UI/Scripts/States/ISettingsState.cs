using UnityEngine;

namespace FifthSemester.UI
{
    public interface ISettingsState
    {
        void EnterState(SettingsMenuState settingsMenuState);
        void ExitState(SettingsMenuState settingsMenuState);
    }
}
