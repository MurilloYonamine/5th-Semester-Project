using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;
using FifthSemester.Core.Services;

public class GameTransitioner : MonoBehaviour {
    [SerializeField] private VideoPlayer _videoPlayer;
    [SerializeField] private GameObject _videoCanvas;

    private IFadeService _fadeService;

    private void Start() {
        _fadeService = ServiceLocator.Get<IFadeService>();
        _videoCanvas.SetActive(false);
        _videoPlayer.GetComponent<CanvasGroup>().alpha = 0f;
    }

    public void StartGameSequence() {
        _videoPlayer.GetComponent<CanvasGroup>().alpha = 1f;
        _fadeService.FadeOut(1.0f, OnFadeOutCompleted);
        _videoPlayer.loopPointReached -= OnVideoFinished;
        _videoPlayer.loopPointReached += OnVideoFinished;
    }

    private void OnFadeOutCompleted() {
        _videoCanvas.SetActive(true);
        _videoPlayer.Prepare();
        _videoPlayer.prepareCompleted -= OnVideoPrepared;
        _videoPlayer.prepareCompleted += OnVideoPrepared;
    }

    private void OnVideoPrepared(VideoPlayer source) {
        source.prepareCompleted -= OnVideoPrepared;
        source.Play();
        _fadeService.FadeIn(1.0f);
    }

    private void OnVideoFinished(VideoPlayer source) {
        source.loopPointReached -= OnVideoFinished;
        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.LoadScene("Game");
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
        if (scene.name != "Game") {
            return;
        }

        SceneManager.sceneLoaded -= OnSceneLoaded;
        _fadeService.FadeIn(1.0f);
    }
}