// autor: Murillo Gomes Yonamine
// data: 29/03/2026

using FifthSemester.Core.States;
using Sirenix.OdinInspector;
using System;
using UnityEngine;

namespace FifthSemester.Core.Managers {
    public class GameStateManager : MonoBehaviour {
        public static GameStateManager Instance { get; private set; }

        [EnumPaging]
        public GameState CurrentState { get; private set; }

        public static event Action<GameState> OnStateChanged;

        private const string GAME_STATE_TAG = "<color=blue>[GameState]:</color>";

        private void Awake() {
            if (Instance == null) {
                Instance = this;
            }
            else {
                Destroy(gameObject);
            }
        }

        public void ChangeState(GameState newState) {
            if (CurrentState == newState) return;

            CurrentState = newState;
            OnStateChanged?.Invoke(newState);

            Debug.Log($"{GAME_STATE_TAG} Mudou para: {newState}");
        }
    }
}
