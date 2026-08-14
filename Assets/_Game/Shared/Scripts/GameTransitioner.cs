using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;
using FifthSemester.Core.Services;

namespace FifthSemester.Shared
{
    public class GameTransitioner : MonoBehaviour
    {
        [SerializeField] private VideoPlayer _videoPlayer;
        [SerializeField] private GameObject _videoCanvas;
        [Header("Menu Music")]
        [SerializeField] private string _menuMusicFilePath = "Audio/menu_musica";

        private IFadeService _fadeService;
        private bool _isLoadingGame;

        private void Start()
        {
            _fadeService = ServiceLocator.Get<IFadeService>();
            _videoCanvas.SetActive(false);
            _videoPlayer.GetComponent<CanvasGroup>().alpha = 0f;
        }

        private void Update()
        {
            if (_videoCanvas == null || !_videoCanvas.activeSelf)
            {
                return;
            }

            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter) ||
                Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Escape) ||
                Input.GetKeyDown(KeyCode.E) ||
                Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1) ||
                (UnityEngine.InputSystem.Keyboard.current != null && (
                    UnityEngine.InputSystem.Keyboard.current.enterKey.wasPressedThisFrame ||
                    UnityEngine.InputSystem.Keyboard.current.numpadEnterKey.wasPressedThisFrame ||
                    UnityEngine.InputSystem.Keyboard.current.spaceKey.wasPressedThisFrame ||
                    UnityEngine.InputSystem.Keyboard.current.escapeKey.wasPressedThisFrame ||
                    UnityEngine.InputSystem.Keyboard.current.eKey.wasPressedThisFrame)) ||
                (UnityEngine.InputSystem.Gamepad.current != null && (
                    UnityEngine.InputSystem.Gamepad.current.buttonEast.wasPressedThisFrame ||
                    UnityEngine.InputSystem.Gamepad.current.buttonSouth.wasPressedThisFrame ||
                    UnityEngine.InputSystem.Gamepad.current.startButton.wasPressedThisFrame)))
            {
                SkipVideo();
            }
        }

        public void StartGameSequence()
        {
            if (!string.IsNullOrWhiteSpace(_menuMusicFilePath))
            {
                IAudioService audioService = ServiceLocator.Get<IAudioService>();
                audioService?.StopTrack(_menuMusicFilePath);
            }

            _isLoadingGame = false;
            _videoPlayer.GetComponent<CanvasGroup>().alpha = 1f;
            _fadeService.FadeOut(1.0f, OnFadeOutCompleted);
            _videoPlayer.loopPointReached -= OnVideoFinished;
            _videoPlayer.loopPointReached += OnVideoFinished;
        }

        public void SkipVideo()
        {
            if (_isLoadingGame)
            {
                return;
            }

            _fadeService.FadeOut(1.0f, LoadGameScene);
        }

        private void OnFadeOutCompleted()
        {
            _videoCanvas.SetActive(true);
            _videoPlayer.Prepare();
            _videoPlayer.prepareCompleted -= OnVideoPrepared;
            _videoPlayer.prepareCompleted += OnVideoPrepared;
        }

        private void OnVideoPrepared(VideoPlayer source)
        {
            source.prepareCompleted -= OnVideoPrepared;
            source.Play();
            _fadeService.FadeIn(1.0f);
        }

        private void OnVideoFinished(VideoPlayer source)
        {
            LoadGameScene();
        }

        private void LoadGameScene()
        {
            if (_isLoadingGame)
            {
                return;
            }

            _isLoadingGame = true;

            if (!string.IsNullOrWhiteSpace(_menuMusicFilePath))
            {
                IAudioService audioService = ServiceLocator.Get<IAudioService>();
                audioService?.StopTrack(_menuMusicFilePath, immediate: true);
            }

            if (_videoPlayer != null)
            {
                _videoPlayer.loopPointReached -= OnVideoFinished;
                _videoPlayer.prepareCompleted -= OnVideoPrepared;
                _videoPlayer.Stop();
            }

            if (_videoCanvas != null)
            {
                _videoCanvas.SetActive(false);
            }

            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneLoaded += OnSceneLoaded;
            SceneManager.LoadScene("Game");
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.name != "Game")
            {
                return;
            }

            SceneManager.sceneLoaded -= OnSceneLoaded;
            
            var activeFadeService = ServiceLocator.Get<IFadeService>();
            if (activeFadeService != null)
            {
                activeFadeService.FadeIn(1.0f);
            }
            else
            {
                _fadeService?.FadeIn(1.0f);
            }
        }
    }
}
