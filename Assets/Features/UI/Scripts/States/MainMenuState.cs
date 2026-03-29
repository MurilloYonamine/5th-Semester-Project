// autor: Murillo Gomes Yonamine
// data: 15/03/2026

using FifthSemester.Core.Managers;
using Sirenix.OdinInspector;
using System.Collections;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace FifthSemester.UI {
    public class MainMenuState : MonoBehaviour, IMenuState {

        [Title("Configurações de Carregamento")]
        [SerializeField, ValueDropdown("GetSceneNames")]
        private string _gameplayLevelName;

        [field: SerializeField] public MenuManager MenuManager { get; private set; }

        public void EnterState(MenuManager menuManager) {
            gameObject.SetActive(true);
        }
        public void ExitState(MenuManager menuManager) {
            gameObject.SetActive(false);
        }

        #region Button Callbacks
        public void OpenPlay() {
            SceneLoaderManager.Instance.LoadNewLevel(_gameplayLevelName);
        }
        public void OpenMainMenu(GameObject caller) {
            if (caller != null)
                EventSystem.current.SetSelectedGameObject(caller);
            MenuManager.ChangeState(MenuManager.MainMenuState);
        }
        public void OpenSettings(GameObject caller) {
            if (caller != null)
                EventSystem.current.SetSelectedGameObject(caller);
            MenuManager.ChangeState(MenuManager.SettingsMenuState);
        }
        public void OpenCredits(GameObject caller) {
            if (caller != null)
                EventSystem.current.SetSelectedGameObject(caller);
            MenuManager.ChangeState(MenuManager.CreditsMenuState);
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
            MenuManager.ChangeState(MenuManager.MainMenuState);
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
