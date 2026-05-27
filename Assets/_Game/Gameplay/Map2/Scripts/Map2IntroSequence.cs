using System.Collections;
using FifthSemester.Core.Services;
using FifthSemester.Core.States;
using UnityEngine;
using UnityEngine.Playables;

namespace FifthSemester.Gameplay.Map2 {
    public class Map2IntroSequence : MonoBehaviour {
        [Header("Fade")]
        [SerializeField] private float _fadeDuration = 1f;

        [Header("Timeline")]
        [SerializeField] private PlayableDirector _introTimeline;

        private bool _isRunning;

        private void Awake() {
            if (_isRunning) {
                return;
            }

            StartCoroutine(PlayFade());
        }

        private IEnumerator PlayFade() {
            _isRunning = true;

            IFadeService fadeService = null;
            ServiceLocator.TryGet<IFadeService>(out fadeService);

            bool fadeCompleted = false;
            fadeService.FadeIn(_fadeDuration, () => fadeCompleted = true);

            while (!fadeCompleted) {
                yield return null;
            }

            _isRunning = false;
        }
    }
}