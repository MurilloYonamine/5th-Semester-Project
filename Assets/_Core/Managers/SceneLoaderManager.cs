using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using FifthSemester.Core.States;
using Sirenix.OdinInspector;
using System.Linq;


#if UNITY_EDITOR
using UnityEditor;
#endif

namespace FifthSemester.Core.Managers {
    public class SceneLoaderManager : MonoBehaviour {
        public static SceneLoaderManager Instance { get; private set; }

        [Title("Configurações de Carga")]
        [SerializeField, Required]
        [ValueDropdown("GetSceneNames")] 
        private string _initialLevelName;

        [SerializeField, Required]
        [ValueDropdown("GetSceneNames")]
        private string _bootstrapSceneName;

        [Title("Status do Sistema")]
        [ShowInInspector, ReadOnly] 
        private string _currentLoadedLevel;

        private const string SCENE_LOADER_TAG = "<color=yellow>[SceneLoader]:</color>";

        private void Awake() {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }
        private void Start() {
            string activeSceneName = SceneManager.GetActiveScene().name;

            if (activeSceneName != _bootstrapSceneName) {
                _currentLoadedLevel = activeSceneName;

                Debug.Log($"{SCENE_LOADER_TAG} Modo teste ativo. Cena atual: {activeSceneName}");
                return;
            }

            if (!string.IsNullOrEmpty(_initialLevelName)) {
                StartCoroutine(LoadInitialSequence(_initialLevelName, ""));
            }
        }
        public void LoadNewLevel(string levelName) {
            if (levelName == _currentLoadedLevel || SceneManager.GetSceneByName(levelName).isLoaded) {
                Debug.LogWarning($"{SCENE_LOADER_TAG} A cena '{levelName}' já está carregada.");
                return;
            }
            StartCoroutine(LoadSequence(levelName));
        }

        private IEnumerator LoadInitialSequence(string levelToLoad, string extraSceneToUnload) {
            AsyncOperation loadOp = SceneManager.LoadSceneAsync(levelToLoad, LoadSceneMode.Additive);
            while (!loadOp.isDone) yield return null;

            _currentLoadedLevel = levelToLoad;

            Scene loadedScene = SceneManager.GetSceneByName(levelToLoad);
            if (loadedScene.IsValid() && loadedScene.isLoaded) {
                SceneManager.SetActiveScene(loadedScene);
            }

            if (!string.IsNullOrEmpty(extraSceneToUnload)) {
                yield return SceneManager.UnloadSceneAsync(extraSceneToUnload);
            }

            GameStateManager.Instance.SetState(GameState.Gameplay);
            Debug.Log($"{SCENE_LOADER_TAG} Inicialização concluída: {levelToLoad}");
        }

        private IEnumerator LoadSequence(string levelName) {
            GameStateManager.Instance.SetState(GameState.Pause);

            AsyncOperation loadOp = SceneManager.LoadSceneAsync(levelName, LoadSceneMode.Additive);
            while (!loadOp.isDone) yield return null;

            Scene newScene = SceneManager.GetSceneByName(levelName);

            if (newScene.IsValid() && newScene.isLoaded) {
                SceneManager.SetActiveScene(newScene);
            }

            if (!string.IsNullOrEmpty(_currentLoadedLevel)) {
                Debug.Log($"{SCENE_LOADER_TAG} Descarregando cena: {_currentLoadedLevel}");

                AsyncOperation unloadOp = SceneManager.UnloadSceneAsync(_currentLoadedLevel);
                if (unloadOp != null) {
                    while (!unloadOp.isDone) yield return null;
                }
            }

            _currentLoadedLevel = levelName;

            GameStateManager.Instance.SetState(GameState.Gameplay);
            Debug.Log($"{SCENE_LOADER_TAG} {levelName} carregado com sucesso!");
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
