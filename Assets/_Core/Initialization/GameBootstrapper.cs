using UnityEngine;
using FifthSemester.Core.Managers; 

namespace FifthSemester.Core.Initialization {
    public static class GameBootstrapper {
        private const string BOOTSTRAP_TAG = "<color=cyan>[Bootstrap]:</color>";

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]

        public static void Initialize() {
            GameObject coreSystemsPrefab = Resources.Load<GameObject>("CoreSystems");

            if (coreSystemsPrefab == null) {
                Debug.LogError($"{BOOTSTRAP_TAG} O Prefab 'CoreSystems' não foi encontrado na pasta Resources!");
                return;
            }

            GameObject instance = Object.Instantiate(coreSystemsPrefab);
            instance.name = "[CORE SYSTEMS]";
            Object.DontDestroyOnLoad(instance);

            Debug.Log($"{BOOTSTRAP_TAG} Sistemas Globais Inicializados.");
        }
    }
}
