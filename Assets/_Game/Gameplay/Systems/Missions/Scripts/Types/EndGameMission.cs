using FifthSemester.Core.Events;
using FifthSemester.Core.Services;
using FifthSemester.Core.States;

using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

namespace FifthSemester.Gameplay {
    public class EndGameMission : MissionBase {

        private GameObject _instantiatedInstance;
        private VideoPlayer _videoPlayer;
        private readonly string _nextSceneName = "MainMenu";

        public override void StartMission() {
            base.StartMission();

            IGameStateService gameStateService = ServiceLocator.Get<IGameStateService>();
            gameStateService?.ChangeState(GameState.Cutscene);

            if (_definition.EndGamePrefab != null) {
                _instantiatedInstance = Object.Instantiate(_definition.EndGamePrefab);
                _videoPlayer = _instantiatedInstance.GetComponentInChildren<VideoPlayer>();

                if (_videoPlayer != null) {
                    MonoBehaviour coroutineRunner = _instantiatedInstance.GetComponent<MonoBehaviour>()
                                                    ?? _videoPlayer.GetComponentInParent<MonoBehaviour>();

                    if (coroutineRunner == null)
                        coroutineRunner = Object.FindAnyObjectByType<StoryManager>();

                    coroutineRunner.StartCoroutine(PlayVideoDelayed());
                }
                else {
                    Debug.LogError("[EndGameMission] Prefab não contém um VideoPlayer!");
                    FinishGame();
                }
            }
        }

        private IEnumerator PlayVideoDelayed() {
            yield return null;
            _videoPlayer.loopPointReached += OnVideoEnded;
            _videoPlayer.Play();
        }

        private void OnVideoEnded(VideoPlayer vp) {
            _videoPlayer.loopPointReached -= OnVideoEnded;
            FinishGame();
        }

        private void FinishGame() {
            Complete();
            SceneManager.LoadScene(_nextSceneName);
        }

        public override void Cleanup() {
            if (_videoPlayer != null) {
                _videoPlayer.loopPointReached -= OnVideoEnded;
            }
            if (_instantiatedInstance != null) {
                Object.Destroy(_instantiatedInstance);
            }
            base.Cleanup();
        }
    }
}
