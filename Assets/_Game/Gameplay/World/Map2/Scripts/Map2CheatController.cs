using UnityEngine;
using FifthSemester.Core.Services;

namespace FifthSemester.Gameplay {
    public class Map2CheatController : MonoBehaviour {
        public static bool IsCheatActive { get; private set; }

        [Header("Cheat Settings")]
        [SerializeField] private KeyCode _cheatKey = KeyCode.Alpha9;

        private void Update() {
            if (Input.GetKeyDown(_cheatKey)) {
                TriggerCheat();
            }
        }

        private void TriggerCheat() {
            IsCheatActive = true;
            Debug.Log("<color=cyan>[CHEAT]</color> Ativando cheat para pular Map 2!");

            // 1. Resolver senhas
            var passwordController = FindObjectOfType<Map2PasswordController>();
            if (passwordController != null) {
                passwordController.CheatForceComplete();
                Debug.Log("<color=cyan>[CHEAT]</color> Senhas resolvidas com sucesso!");
            } else {
                Debug.LogWarning("[CHEAT] Map2PasswordController não encontrado na cena!");
            }

            // 2. Desativar enfermeira, limpar chaves anteriores e dar a chave final
            if (ServiceLocator.TryGet<IMap2KeyService>(out var keyService)) {
                keyService.CheatSetKeysCollected();
            } else {
                // Fallback por Find se o ServiceLocator não tiver registrado
                var keyServiceFallback = FindObjectOfType<KeyService>();
                if (keyServiceFallback != null) {
                    keyServiceFallback.CheatSetKeysCollected();
                } else {
                    Debug.LogWarning("[CHEAT] KeyService (IMap2KeyService) não encontrado!");
                }
            }
        }
    }
}
