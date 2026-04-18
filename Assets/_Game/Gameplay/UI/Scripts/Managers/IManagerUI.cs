using UnityEngine;

namespace FifthSemester.UI {
    public interface IManagerUI {
        public static IManagerUI Instance { get; }

        public MainMenuState MainMenuState { get; }
        public SettingsMenuState SettingsMenuState { get; }
        public CreditsMenuState CreditsMenuState { get; }
        public void ChangeState(IMenuState newState);
        public void OnReturn(GameObject caller);
    }
}
