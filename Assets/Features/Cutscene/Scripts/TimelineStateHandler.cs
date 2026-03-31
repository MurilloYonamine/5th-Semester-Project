// autor: Murillo Gomes Yonamine
// data: 31/03/2026

using FifthSemester.Core.Managers;
using FifthSemester.Core.States;
using UnityEngine;
using UnityEngine.Playables;

namespace FifthSemester.Cutscene {
    public class TimelineStateHandler : MonoBehaviour {
        [SerializeField] private PlayableDirector _director;

        private void Start() {
            if (GameStateManager.Instance != null) {
                GameStateManager.Instance.ChangeState(GameState.Cutscene);
            }
        }

        public void OnCutsceneFinished() {
            if (GameStateManager.Instance != null) {
                GameStateManager.Instance.ChangeState(GameState.Gameplay);
            }

            Debug.Log("Cutscene finalizada, voltando para o Gameplay.");
        }
    }
}
