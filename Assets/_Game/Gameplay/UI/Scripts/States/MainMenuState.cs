// autor: Murillo Gomes Yonamine
// data: 15/03/2026

using Sirenix.OdinInspector;
using Sirenix.Serialization;
using System.Collections;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace FifthSemester.UI {
    public class MainMenuState : MonoBehaviour, IMenuState {

        [SerializeField] private bool _isMainMenu = false;

        [ShowIf("_isMainMenu")]
        [Title("Configurações de Carregamento")]
        [SerializeField, ValueDropdown("GetSceneNames")]
        private string _gameplayLevelName;

        [SerializeField] private Button _playButton;

        public void EnterState() {
            gameObject.SetActive(true);

            if(_playButton != null)
                EventSystem.current.SetSelectedGameObject(_playButton.gameObject);
        }
        public void ExitState() {
            gameObject.SetActive(false);
        }

        #region Button Callbacks

        public void OpenMainMenu(GameObject caller) {
            if (caller != null)
                EventSystem.current.SetSelectedGameObject(caller);
            MenuManager.Instance.ChangeState(MenuManager.Instance.MainMenuState);
        }
        public void OpenSettings(GameObject caller) {
            if (caller != null)
                EventSystem.current.SetSelectedGameObject(caller);
            MenuManager.Instance.ChangeState(MenuManager.Instance.SettingsMenuState);
        }
        public void OpenCredits(GameObject caller) {
            if (caller != null)
                EventSystem.current.SetSelectedGameObject(caller);
            MenuManager.Instance.ChangeState(MenuManager.Instance.CreditsMenuState);
        }
        public void Exit() {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
                        Application.Quit();
#endif
        }
        #endregion

        public void OnReturn(GameObject caller) {
            MenuManager.Instance.ChangeState(MenuManager.Instance.MainMenuState);
            EventSystem.current.SetSelectedGameObject(caller);
        }
        public override string ToString() {
            return "Main Menu State";
        }
#if UNITY_EDITOR
        private static IEnumerable GetSceneNames() {
            return EditorBuildSettings.scenes
                .Where(s => s.enabled)
                .Select(s => System.IO.Path.GetFileNameWithoutExtension(s.path));
        }
#endif
    }
}
